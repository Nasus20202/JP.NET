namespace Lab3.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public Guid Id { get; } = Guid.NewGuid();
}
