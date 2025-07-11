using System;
using System.Collections.Generic;

// Componente
public interface IComponent
{
    void Operation();
}

// Hoja
public class Leaf : IComponent
{
    public void Operation() => Console.WriteLine("Hoja");
}

// Compuesto
public class Composite : IComponent
{
    private List<IComponent> _children = new List<IComponent>();
    public void Add(IComponent component) => _children.Add(component);
    public void Operation()
    {
        Console.WriteLine("Composite");
        foreach (var child in _children)
            child.Operation();
    }
}

// Ejemplo de uso
// var leaf = new Leaf();
// var composite = new Composite();
// composite.Add(leaf);
// composite.Operation(); // Output: Composite\nHoja
