using Autofac;
using Lab3.Calculator;
using Microsoft.Extensions.Configuration;

namespace Lab3.Tests.Calculator;

public class StateCalcTests
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
    public void StateCalc_IsSingleton_ReturnsSameInstance(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var calc1 = container.ResolveNamed<ICalculator>(DI.StateCalc);
        var calc2 = container.ResolveNamed<ICalculator>(DI.StateCalc);

        // Assert
        Assert.Same(calc1, calc2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StateCalc_IsSingleton_SharesStateAcrossResolves(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var calc1 = container.ResolveNamed<ICalculator>(DI.StateCalc);
        var result1 = calc1.Eval("First", "Call");

        var calc2 = container.ResolveNamed<ICalculator>(DI.StateCalc);
        var result2 = calc2.Eval("Second", "Call");

        // Assert
        Assert.Equal("First Call 0", result1);
        Assert.Equal("Second Call 1", result2);
        Assert.Same(calc1, calc2);
    }
}
