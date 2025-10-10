using Autofac;
using Lab3.Transaction;
using Microsoft.Extensions.Configuration;

namespace Lab3.Tests.Transaction;

public class StepOneServiceTests
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
    [InlineData(false)]
    public void StepOneService_InTransactionScope_ResolvesCorrectly(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);
        using var transactionScope = container.BeginLifetimeScope(
            TransactionProcessor.TransactionTag
        );

        // Act
        var step1 = transactionScope.Resolve<StepOneService>();

        // Assert
        Assert.NotNull(step1);
    }

    [Theory]
    [InlineData(false)]
    public void StepOneService_HasTransactionContext(bool useDeclarative)
    {
        // Arrange
        using var container = CreateContainer(useDeclarative);
        using var transactionScope = container.BeginLifetimeScope(
            TransactionProcessor.TransactionTag
        );

        // Act
        var step1 = transactionScope.Resolve<StepOneService>();
        var context = GetPrivateField<ITransactionContext>(step1, "_context");

        // Assert
        Assert.NotNull(context);
        Assert.NotEqual(Guid.Empty, context.TransactionId);
    }

    private T GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType()
            .GetField(
                fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

        if (field == null)
            throw new ArgumentException($"Field {fieldName} not found");

        return (T)field.GetValue(obj)!;
    }
}
