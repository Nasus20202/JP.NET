using Autofac;
using Lab3.Calculator;
using Microsoft.Extensions.Configuration;

namespace Lab3.Tests.Calculator;

public class PlusCalcTests
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
    public void PlusCalc_AddsNumbers(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);

        // Act
        var calc = container.ResolveNamed<ICalculator>(DI.PlusCalc);
        var result = calc.Eval("5", "3");

        // Assert
        Assert.Equal("8", result);
    }
}
