using Lab3.Calculator;

namespace Lab3.Worker;

public class Worker3 : IWorker
{
    public ICalculator? Calculator { get; set; }

    public string Work(string a, string b)
    {
        return Calculator?.Eval(a, b) ?? throw new InvalidOperationException("Calculator not set");
    }
}
