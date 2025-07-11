# Strategy Pattern

The Strategy pattern defines a family of algorithms, encapsulates each one, and makes them interchangeable. Strategy lets the algorithm vary independently from clients that use it.

## Structure

- **Strategy**: Declares an interface common to all supported algorithms.
- **ConcreteStrategy**: Implements the algorithm using the Strategy interface.
- **Context**: Maintains a reference to a Strategy object.

## Example

```csharp
var context = new Context(new AddStrategy());
Console.WriteLine(context.Execute(2, 3)); // Output: 5
context = new Context(new SubtractStrategy());
Console.WriteLine(context.Execute(5, 2)); // Output: 3
```

## When to use

- When you have many related classes that differ only in their behavior.
- When you need different variants of an algorithm.
