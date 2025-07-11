# Specification Pattern

The Specification pattern allows you to encapsulate business rules and reuse them by combining simple specifications into more complex ones.

## Structure

- **ISpecification**: Interface for business rules.
- **EvenNumberSpecification**: Example implementation.

## Example

```csharp
var spec = new EvenNumberSpecification();
Console.WriteLine(spec.IsSatisfiedBy(2)); // Output: True
```

## When to use

- When you want to encapsulate and reuse business rules.
- To combine simple rules into complex specifications.
