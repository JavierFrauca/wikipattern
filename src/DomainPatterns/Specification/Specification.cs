using System;

// Specification Pattern
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T item);
}

public class EvenNumberSpecification : ISpecification<int>
{
    public bool IsSatisfiedBy(int item) => item % 2 == 0;
}

// Ejemplo de uso
// var spec = new EvenNumberSpecification();
// Console.WriteLine(spec.IsSatisfiedBy(2)); // Output: True
