# Value Object Pattern

The Value Object pattern represents an object that is defined by its attributes rather than a unique identity.

## Structure

- **Money**: Example value object with equality based on value.

## Example

```csharp
var m1 = new Money(10, "USD");
var m2 = new Money(10, "USD");
Console.WriteLine(m1.Equals(m2)); // Output: True
```

## When to use

- When you need objects that are interchangeable based on their values.
- To model concepts that don't require identity.
