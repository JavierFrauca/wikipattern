# Factory Method

## Description
The Factory Method is a creational design pattern that provides an interface for creating objects in a superclass, but allows subclasses to alter the type of objects that will be created.

## When to use
- When a class can't anticipate the class of objects it must create.
- To delegate the responsibility of instantiation to subclasses.

## When NOT to use
- When object creation is simple and doesn't require extra logic.
- When you don't need to vary the product class.

## Implementation
```csharp
// Product interface
public interface ITransport
{
    void Deliver();
}

// Concrete Product
public class Truck : ITransport
{
    public void Deliver() => Console.WriteLine("Deliver by land in a box");
}

// Creator
public abstract class Logistics
{
    public abstract ITransport CreateTransport();
}

// Concrete Creator
public class RoadLogistics : Logistics
{
    public override ITransport CreateTransport() => new Truck();
}
```

## Practical example
```csharp
var logistics = new RoadLogistics();
ITransport transport = logistics.CreateTransport();
transport.Deliver(); // Output: Deliver by land in a box
```

## Advantages
- Promotes code reusability and flexibility
- Supports Open/Closed Principle

## Disadvantages
- Can introduce extra classes and complexity

## Related patterns
- Abstract Factory
- Prototype
