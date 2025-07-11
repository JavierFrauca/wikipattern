using System;

// Iterador
public interface IIterator<T>
{
    bool HasNext();
    T Next();
}

// Colección
public class NumberCollection
{
    private int[] _numbers;
    public NumberCollection(int[] numbers) => _numbers = numbers;
    public IIterator<int> GetIterator() => new NumberIterator(_numbers);
}

// Iterador concreto
public class NumberIterator : IIterator<int>
{
    private int[] _numbers;
    private int _position = 0;
    public NumberIterator(int[] numbers) => _numbers = numbers;
    public bool HasNext() => _position < _numbers.Length;
    public int Next() => _numbers[_position++];
}

// Ejemplo de uso
// var collection = new NumberCollection(new[] { 1, 2, 3 });
// var iterator = collection.GetIterator();
// while (iterator.HasNext()) Console.WriteLine(iterator.Next()); // Output: 1 2 3
