using Autofac;
using Microsoft.Extensions.Configuration;
using Worker2 = Lab3.Worker.Worker2;

namespace Lab3.Tests.Worker;

public class Worker2Tests
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
    public void Worker2_WithPlusCalc_AddsStrings(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker = container.Resolve<Worker2>();
        var result = worker.Work("5", "3");

        // Assert
        Assert.Equal("8", result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Worker2_WithStateCalc_ReturnsWithState(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker = container.ResolveNamed<Worker2>(DI.StateWorker);
        var result = worker.Work("X", "Y");

        // Assert
        Assert.Equal("X Y 0", result);
    }
}
