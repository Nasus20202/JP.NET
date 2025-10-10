namespace Lab3.Calculator;

public class StateCalc : ICalculator
{
    private int _state;

    public StateCalc(int state) => _state = state;

    public string Eval(string a, string b)
    {
        return $"{a} {b} {_state++}";
    }
}
