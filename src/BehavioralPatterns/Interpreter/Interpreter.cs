using System;

// Expresión abstracta
public interface IExpression
{
    int Interpret();
}

// Expresión numérica
public class NumberExpression : IExpression
{
    private int _number;
    public NumberExpression(int number) => _number = number;
    public int Interpret() => _number;
}

// Suma
public class AddExpression : IExpression
{
    private IExpression _left, _right;
    public AddExpression(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }
    public int Interpret() => _left.Interpret() + _right.Interpret();
}

// Ejemplo de uso
// var expr = new AddExpression(new NumberExpression(2), new NumberExpression(3));
// Console.WriteLine(expr.Interpret()); // Output: 5
