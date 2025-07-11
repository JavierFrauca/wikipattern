using System;
using System.Collections.Generic;

// Flyweight
public interface IFlyweight
{
    void Operation(string extrinsicState);
}

// Flyweight concreto
public class ConcreteFlyweight : IFlyweight
{
    private string _intrinsicState;
    public ConcreteFlyweight(string intrinsicState) => _intrinsicState = intrinsicState;
    public void Operation(string extrinsicState) => Console.WriteLine($"Intrinsic: {_intrinsicState}, Extrinsic: {extrinsicState}");
}

// Fabrica de Flyweights
public class FlyweightFactory
{
    private Dictionary<string, IFlyweight> _flyweights = new();
    public IFlyweight GetFlyweight(string key)
    {
        if (!_flyweights.ContainsKey(key))
            _flyweights[key] = new ConcreteFlyweight(key);
        return _flyweights[key];
    }
}

// Ejemplo de uso
// var factory = new FlyweightFactory();
// var flyweight = factory.GetFlyweight("A");
// flyweight.Operation("estado externo"); // Output: Intrinsic: A, Extrinsic: estado externo
