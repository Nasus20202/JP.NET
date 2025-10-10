using Autofac;
using Lab3.Calculator;
using Lab3.Transaction;
using Microsoft.Extensions.Configuration;
using Worker1 = Lab3.Worker.Worker;
using Worker2 = Lab3.Worker.Worker2;
using Worker3 = Lab3.Worker.Worker3;

namespace Lab3.Tests;

public class IntegrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FullStack_AllComponentsResolveCorrectly(bool useDeclarative)
    {
        // Arrange
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

        using var container = builder.Build();

        // Act & Assert
        Assert.NotNull(container.Resolve<Worker1>());
        Assert.NotNull(container.Resolve<Worker2>());
        Assert.NotNull(container.Resolve<Worker3>());
        Assert.NotNull(container.ResolveNamed<ICalculator>(DI.CatCalc));
        Assert.NotNull(container.ResolveNamed<ICalculator>(DI.PlusCalc));
        Assert.NotNull(container.ResolveNamed<ICalculator>(DI.StateCalc));
    }

    [Fact]
    public void ImperativeConfiguration_TransactionComponents_ResolveCorrectly()
    {
        // Arrange
        var builder = new ContainerBuilder();
        DI.ConfigureAll(builder);
        using var container = builder.Build();

        // Act & Assert
        Assert.NotNull(container.Resolve<TransactionProcessor>());

        using var scope = container.BeginLifetimeScope(TransactionProcessor.TransactionTag);
        Assert.NotNull(scope.Resolve<StepOneService>());
        Assert.NotNull(scope.Resolve<StepTwoService>());
        Assert.NotNull(scope.Resolve<ITransactionContext>());
    }
}
