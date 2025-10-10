using Autofac;

namespace Lab3.Transaction;

public class TransactionProcessor
{
    public static readonly string TransactionTag = "transaction";

    private readonly ILifetimeScope _scope;

    public TransactionProcessor(ILifetimeScope scope) => _scope = scope;

    public void ProcessTransaction()
    {
        using var transactionScope = _scope.BeginLifetimeScope(TransactionTag);
        Console.WriteLine("Starting new transaction");
        var step1 = transactionScope.Resolve<StepOneService>();
        var step2 = transactionScope.Resolve<StepTwoService>();
        step1.Execute();
        step2.Execute();
        Console.WriteLine("Transaction completed");
    }
}
