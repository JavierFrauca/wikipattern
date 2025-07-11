using System;

// Implementador
public interface IImplementor
{
    void OperationImpl();
}

// Implementador concreto
public class ConcreteImplementorA : IImplementor
{
    public void OperationImpl() => Console.WriteLine("Implementación A");
}

// Abstracción
public abstract class Abstraction
{
    protected IImplementor _implementor;
    public Abstraction(IImplementor implementor) => _implementor = implementor;
    public abstract void Operation();
}

// Abstracción refinada
public class RefinedAbstraction : Abstraction
{
    public RefinedAbstraction(IImplementor implementor) : base(implementor) { }
    public override void Operation() => _implementor.OperationImpl();
}

// Ejemplo de uso
// var impl = new ConcreteImplementorA();
// var abs = new RefinedAbstraction(impl);
// abs.Operation(); // Output: Implementación A
