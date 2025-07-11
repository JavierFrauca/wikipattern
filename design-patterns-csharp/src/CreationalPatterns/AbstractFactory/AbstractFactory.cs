using System;

// Interfaz para el producto
public interface IAbstractProduct
{
    void Operate();
}

// Producto concreto A
public class ConcreteProductA : IAbstractProduct
{
    public void Operate() => Console.WriteLine("Operando producto A");
}

// Producto concreto B
public class ConcreteProductB : IAbstractProduct
{
    public void Operate() => Console.WriteLine("Operando producto B");
}

// Interfaz para la fábrica abstracta
public interface IAbstractFactory
{
    IAbstractProduct CreateProduct(string type);
}

// Fábrica concreta
public class ConcreteFactory : IAbstractFactory
{
    public IAbstractProduct CreateProduct(string type)
    {
        return type switch
        {
            "A" => new ConcreteProductA(),
            "B" => new ConcreteProductB(),
            _ => throw new ArgumentException("Tipo no soportado")
        };
    }
}

// Ejemplo de uso
// var factory = new ConcreteFactory();
// IAbstractProduct product = factory.CreateProduct("A");
// product.Operate(); // Output: Operando producto A
// product = factory.CreateProduct("B");
// product.Operate(); // Output: Operando producto B
