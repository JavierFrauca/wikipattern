# Visitor Pattern

The Visitor pattern lets you define new operations on objects without changing their classes. It separates an algorithm from the object structure it operates on.

## Structure

- **Visitor**: Declares a visit operation for each type of element.
- **ConcreteVisitor**: Implements each operation declared by Visitor.
- **Element**: Defines an Accept method that takes a visitor.
- **ConcreteElement**: Implements the Accept method.

## Example

```csharp
var element = new ConcreteElement();
var visitor = new ConcreteVisitor();
element.Accept(visitor); // Output: Visitando elemento
```

## When to use

- When you need to perform operations across a set of objects with different interfaces.
- When the object structure rarely changes, but operations on it change frequently.
