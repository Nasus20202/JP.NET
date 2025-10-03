using System.Text.Json;
using ZtmBus.Models;

namespace ZtmBus.Services;

public class ZtmApiService
{
    private readonly HttpClient _httpClient;
    private const string StopsUrl =
        "https://ckan.multimediagdansk.pl/dataset/c24aa637-3619-4dc2-a171-a23eec8f2172/resource/4c4025f0-01bf-41f7-a39f-d156d201b82b/download/stops.json";
    private const string DeparturesUrl = "https://ckan2.multimediagdansk.pl/departures";

    public ZtmApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<StopInfo>> GetStopsAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(StopsUrl);
            var stopsResponse = JsonSerializer.Deserialize<Dictionary<string, StopsData>>(response);

            if (stopsResponse != null && stopsResponse.Any())
            {
                // Pobierz najnowsze dane (ostatni klucz w słowniku)
                var latestData = stopsResponse.Values.Last();
                return latestData.Stops;
            }

            return new List<StopInfo>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Błąd podczas pobierania listy przystanków: {ex.Message}", ex);
        }
    }

    public async Task<DeparturesResponse> GetDeparturesAsync(int stopId)
    {
        try
        {
            var url = $"{DeparturesUrl}?stopId={stopId}";
            var response = await _httpClient.GetStringAsync(url);
            var departures = JsonSerializer.Deserialize<DeparturesResponse>(response);

            return departures ?? new DeparturesResponse();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Błąd podczas pobierania odjazdów dla przystanku {stopId}: {ex.Message}",
                ex
            );
        }
    }

    public List<StopInfo> FindStopsByName(List<StopInfo> stops, string searchTerm)
    {
        var normalizedSearchTerm = NormalizeString(searchTerm);

        return stops
            .Where(stop =>
                NormalizeString(stop.StopDesc)
                    .Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase)
                || (
                    stop.StopName != null
                    && NormalizeString(stop.StopName)
                        .Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase)
                )
            )
            .OrderBy(stop => stop.StopDesc)
            .ToList();
    }

    private static string NormalizeString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Usuń polskie znaki diakrytyczne dla lepszego wyszukiwania
        return input
            .Replace("ą", "a")
            .Replace("Ą", "A")
            .Replace("ć", "c")
            .Replace("Ć", "C")
            .Replace("ę", "e")
            .Replace("Ę", "E")
            .Replace("ł", "l")
            .Replace("Ł", "L")
            .Replace("ń", "n")
            .Replace("Ń", "N")
            .Replace("ó", "o")
            .Replace("Ó", "O")
            .Replace("ś", "s")
            .Replace("Ś", "S")
            .Replace("ź", "z")
            .Replace("Ź", "Z")
            .Replace("ż", "z")
            .Replace("Ż", "Z");
    }
}
