using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using ZtmBus.Models;
using ZtmBus.Services;

namespace ZtmBus.Test;

public class ZtmApiServiceTests
{
    private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json"),
                }
            );
        return handlerMock;
    }

    [Fact]
    public async Task GetStopsAsync_ReturnsStops_WhenApiReturnsValidData()
    {
        // Arrange
        var testStops = new Dictionary<string, StopsData>
        {
            ["2025-10-04"] = new StopsData
            {
                LastUpdate = "2025-10-04 12:00:00",
                Stops = new List<StopInfo>
                {
                    new StopInfo
                    {
                        StopId = 1001,
                        StopDesc = "Politechnika",
                        StopName = "Politechnika",
                        ZoneName = "Gdańsk",
                        Type = "BUS",
                    },
                    new StopInfo
                    {
                        StopId = 1002,
                        StopDesc = "Dworzec Główny",
                        StopName = "Dworzec Główny",
                        ZoneName = "Gdańsk",
                        Type = "BUS_TRAM",
                    },
                },
            },
        };

        var jsonResponse = JsonSerializer.Serialize(testStops);
        var handlerMock = CreateMockHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ZtmApiService(httpClient);

        // Act
        var result = await service.GetStopsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Politechnika", result.First().StopDesc);
        Assert.Equal(1001, result.First().StopId);
    }

    [Fact]
    public async Task GetDeparturesAsync_ReturnsDepartures_WhenApiReturnsValidData()
    {
        // Arrange
        var testDepartures = new DeparturesResponse
        {
            LastUpdate = DateTime.Now,
            Departures = new List<DepartureInfo>
            {
                new DepartureInfo
                {
                    Id = "T123R456",
                    RouteShortName = "150",
                    Headsign = "Brzeźno",
                    EstimatedTime = DateTime.Now.AddMinutes(5),
                    DelayInSeconds = 120,
                    Status = "REALTIME",
                },
            },
        };

        var jsonResponse = JsonSerializer.Serialize(testDepartures);
        var handlerMock = CreateMockHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ZtmApiService(httpClient);

        // Act
        var result = await service.GetDeparturesAsync(1001);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Departures);
        Assert.Equal("150", result.Departures.First().RouteShortName);
        Assert.Equal(120, result.Departures.First().DelayInSeconds);
    }

    [Fact]
    public void FindStopsByName_ReturnsMatchingStops_WhenSearchTermExists()
    {
        // Arrange
        var stops = new List<StopInfo>
        {
            new StopInfo
            {
                StopId = 1,
                StopDesc = "Politechnika",
                ZoneName = "Gdańsk",
            },
            new StopInfo
            {
                StopId = 2,
                StopDesc = "Dworzec Główny",
                ZoneName = "Gdańsk",
            },
            new StopInfo
            {
                StopId = 3,
                StopDesc = "Politechnika Gdańska",
                ZoneName = "Gdańsk",
            },
            new StopInfo
            {
                StopId = 4,
                StopDesc = "Centrum",
                ZoneName = "Gdynia",
            },
        };

        var httpClient = new HttpClient();
        var service = new ZtmApiService(httpClient);

        // Act
        var result = service.FindStopsByName(stops, "Politechnika");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, stop => Assert.Contains("Politechnika", stop.StopDesc));
    }

    [Fact]
    public void FindStopsByName_ReturnsEmptyList_WhenNoMatchFound()
    {
        // Arrange
        var stops = new List<StopInfo>
        {
            new StopInfo
            {
                StopId = 1,
                StopDesc = "Politechnika",
                ZoneName = "Gdańsk",
            },
            new StopInfo
            {
                StopId = 2,
                StopDesc = "Dworzec",
                ZoneName = "Gdańsk",
            },
        };

        var httpClient = new HttpClient();
        var service = new ZtmApiService(httpClient);

        // Act
        var result = service.FindStopsByName(stops, "Nowy Świat");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindStopsByName_HandlesPolishCharacters_WhenSearching()
    {
        // Arrange
        var stops = new List<StopInfo>
        {
            new StopInfo
            {
                StopId = 1,
                StopDesc = "Żabianka",
                ZoneName = "Gdańsk",
            },
            new StopInfo
            {
                StopId = 2,
                StopDesc = "Śródmieście",
                ZoneName = "Gdańsk",
            },
        };

        var httpClient = new HttpClient();
        var service = new ZtmApiService(httpClient);

        // Act
        var result1 = service.FindStopsByName(stops, "Zabianka"); // bez polskich znaków
        var result2 = service.FindStopsByName(stops, "Srodmiescie"); // bez polskich znaków

        // Assert
        Assert.Single(result1);
        Assert.Equal("Żabianka", result1.First().StopDesc);
        Assert.Single(result2);
        Assert.Equal("Śródmieście", result2.First().StopDesc);
    }

    [Fact]
    public async Task GetStopsAsync_ThrowsException_WhenHttpRequestFails()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler("", HttpStatusCode.NotFound);
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ZtmApiService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.GetStopsAsync());
        Assert.Contains("Błąd podczas pobierania listy przystanków", exception.Message);
    }

    [Fact]
    public async Task GetDeparturesAsync_ThrowsException_WhenHttpRequestFails()
    {
        // Arrange
        var handlerMock = CreateMockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handlerMock.Object);
        var service = new ZtmApiService(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => service.GetDeparturesAsync(1001));
        Assert.Contains("Błąd podczas pobierania odjazdów dla przystanku 1001", exception.Message);
    }
}
