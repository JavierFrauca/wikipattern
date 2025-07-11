# Domain Service Pattern

The Domain Service pattern encapsulates domain logic that doesn't naturally fit within a single entity or value object.

## Structure

- **DomainService**: Provides domain operations (e.g., IsValid).

## Example

```csharp
var service = new DomainService();
Console.WriteLine(service.IsValid("abc")); // Output: True
```

## When to use

- When domain logic spans multiple entities or value objects.
- To keep entities and value objects focused on their core responsibilities.
