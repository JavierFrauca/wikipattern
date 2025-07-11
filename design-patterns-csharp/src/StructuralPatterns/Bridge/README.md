# Bridge Pattern

The Bridge pattern decouples an abstraction from its implementation so that the two can vary independently.

## Structure

- **Abstraction**: Defines the abstraction's interface and maintains a reference to an implementor.
- **RefinedAbstraction**: Extends the interface defined by Abstraction.
- **Implementor**: Defines the interface for implementation classes.
- **ConcreteImplementor**: Implements the Implementor interface.

## Example

```csharp
var impl = new ConcreteImplementorA();
var abs = new RefinedAbstraction(impl);
abs.Operation(); // Output: Implementación A
```

## When to use

- When you want to avoid a permanent binding between an abstraction and its implementation.
- When both the abstractions and their implementations should be extensible by subclassing.
