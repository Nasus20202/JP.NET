using Autofac;
using Lab3.Calculator;
using Microsoft.Extensions.Configuration;
using Worker1 = Lab3.Worker.Worker;
using Worker2 = Lab3.Worker.Worker2;
using Worker3 = Lab3.Worker.Worker3;

namespace Lab3.Tests.Worker;

public class WorkerIntegrationTests
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
    public void MultipleWorkersWithStateCalc_ShareSameInstance(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker1 = container.ResolveNamed<Worker1>(DI.StateWorker);
        var result1 = worker1.Work("First", "Worker");

        var worker2 = container.ResolveNamed<Worker2>(DI.StateWorker);
        var result2 = worker2.Work("Second", "Worker");

        var worker3 = container.ResolveNamed<Worker3>(DI.StateWorker);
        var result3 = worker3.Work("Third", "Worker");

        // Assert
        Assert.Equal("First Worker 0", result1);
        Assert.Equal("Second Worker 1", result2);
        Assert.Equal("Third Worker 2", result3);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WorkersWithDifferentCalculators_WorkIndependently(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var worker1WithCat = container.Resolve<Worker1>();
        var worker2WithPlus = container.Resolve<Worker2>();
        var worker3WithCat = container.Resolve<Worker3>();

        var result1 = worker1WithCat.Work("A", "B");
        var result2 = worker2WithPlus.Work("10", "20");
        var result3 = worker3WithCat.Work("X", "Y");

        // Assert
        Assert.Equal("A B", result1);
        Assert.Equal("30", result2);
        Assert.Equal("X Y", result3);
    }
}
