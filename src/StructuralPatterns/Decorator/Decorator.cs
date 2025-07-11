using System;

// Componente
public interface IComponent
{
    string Operation();
}

// Componente concreto
public class ConcreteComponent : IComponent
{
    public string Operation() => "Componente";
}

// Decorador base
public abstract class Decorator : IComponent
{
    protected IComponent _component;
    public Decorator(IComponent component) => _component = component;
    public abstract string Operation();
}

// Decorador concreto
public class ConcreteDecorator : Decorator
{
    public ConcreteDecorator(IComponent component) : base(component) { }
    public override string Operation() => $"Decorado({_component.Operation()})";
}

// Ejemplo de uso
// IComponent component = new ConcreteComponent();
// component = new ConcreteDecorator(component);
// Console.WriteLine(component.Operation()); // Output: Decorado(Componente)
