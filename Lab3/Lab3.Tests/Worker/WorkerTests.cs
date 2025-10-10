using Autofac;
using Lab3.Calculator;
using Microsoft.Extensions.Configuration;
using Worker1 = Lab3.Worker.Worker;

namespace Lab3.Tests.Worker;

public class WorkerTests
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
    public void Worker_WithCatCalc_ConcatenatesStrings(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker = container.Resolve<Worker1>();
        var result = worker.Work("Hello", "World");

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Worker_WithStateCalc_ReturnsWithState(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker = container.ResolveNamed<Worker1>(DI.StateWorker);
        var result = worker.Work("A", "B");

        // Assert
        Assert.Equal("A B 0", result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ResolvingWorker_MultipleTimes_CreatesSeparateInstances(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker1 = container.Resolve<Worker1>();
        var worker2 = container.Resolve<Worker1>();

        // Assert
        Assert.NotSame(worker1, worker2);
    }
}
