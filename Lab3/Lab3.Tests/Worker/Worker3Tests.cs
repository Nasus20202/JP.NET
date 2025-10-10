using Autofac;
using Microsoft.Extensions.Configuration;
using Worker3 = Lab3.Worker.Worker3;

namespace Lab3.Tests.Worker;

public class Worker3Tests
{
    private IContainer CreateContainer(bool useDeclarative)
    {
        var builder = new ContainerBuilder();

        if (useDeclarative)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            DI.ConfigureDeclarative(builder, config);
        }
        else
        {
            DI.ConfigureAll(builder);
        }

        return builder.Build();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Worker3_WithCatCalc_ConcatenatesStrings(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker = container.Resolve<Worker3>();
        var result = worker.Work("Test", "123");

        // Assert
        Assert.Equal("Test 123", result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Worker3_WithStateCalc_ReturnsWithState(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker = container.ResolveNamed<Worker3>(DI.StateWorker);
        var result = worker.Work("M", "N");

        // Assert
        Assert.Equal("M N 0", result);
    }
}
