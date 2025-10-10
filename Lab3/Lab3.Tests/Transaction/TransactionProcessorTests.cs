using Autofac;
using Lab3.Transaction;

namespace Lab3.Tests.Transaction;

public class TransactionProcessorTests
{
    private IContainer CreateContainer()
    {
        var builder = new ContainerBuilder();
        DI.ConfigureAll(builder);
        return builder.Build();
    }

    [Fact]
    public void TransactionProcessor_ProcessesTransaction_Successfully()
    {
        // Arrange
        using var container = CreateContainer();
        var processor = container.Resolve<TransactionProcessor>();

        // Act & Assert
        processor.ProcessTransaction();
    }

    [Fact]
    public void TransactionProcessor_MultipleExecutions_CreateSeparateTransactions()
    {
        // Arrange
        using var container = CreateContainer();
        var processor = container.Resolve<TransactionProcessor>();
        var originalOut = Console.Out;
        var outputs = new List<string>();

        // Act
        try
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);

            processor.ProcessTransaction();
            processor.ProcessTransaction();

            var output = sw.ToString();
            outputs.AddRange(output.Split('\n', StringSplitOptions.RemoveEmptyEntries));
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        // Assert
        var startCount = outputs.Count(line => line.Contains("Starting new transaction"));
        Assert.Equal(2, startCount);
    }
}
