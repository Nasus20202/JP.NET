using Lab3.Calculator;

namespace Lab3.Worker;

public class Worker2 : IWorker
{
    private ICalculator? _calculator;

    public void SetCalculator(ICalculator calculator)
    {
        _calculator = calculator;
    }

    public string Work(string a, string b)
    {
        return _calculator?.Eval(a, b) ?? throw new InvalidOperationException("Calculator not set");
    }
}
