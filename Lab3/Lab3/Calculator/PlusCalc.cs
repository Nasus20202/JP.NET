namespace Lab3.Calculator;

public class PlusCalc : ICalculator
{
    public string Eval(string a, string b)
    {
        return (int.Parse(a) + int.Parse(b)).ToString();
    }
}
