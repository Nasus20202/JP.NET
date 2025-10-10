namespace Lab3.Calculator;

public class CatCalc : ICalculator
{
    public string Eval(string a, string b)
    {
        return $"{a} {b}";
    }
}
