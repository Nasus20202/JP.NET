namespace Lab3.Transaction;

public class TransactionContext : ITransactionContext
{
    public Guid TransactionId { get; } = Guid.NewGuid();

    public TransactionContext()
    {
        Console.WriteLine($"Created new transaction [{TransactionId}]");
    }
}
