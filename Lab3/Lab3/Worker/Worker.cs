using Lab3.Calculator;

namespace Lab3.Worker;

public class Worker : IWorker
{
    private readonly ICalculator _calculator;

    public Worker(ICalculator calculator)
    {
        _calculator = calculator;
    }

    public string Work(string a, string b)
    {
        return _calculator.Eval(a, b);
    }
}
