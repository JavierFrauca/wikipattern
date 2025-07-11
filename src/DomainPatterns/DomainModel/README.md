# Domain Model Pattern

The Domain Model pattern organizes business logic and data as a rich object model, encapsulating state and behavior.

## Structure

- **Product**: Example domain entity with business logic.

## Example

```csharp
var product = new Product { Name = "Libro", Price = 100 };
product.ApplyDiscount(0.1m);
Console.WriteLine(product.Price); // Output: 90
```

## When to use

- When you want to encapsulate business rules and logic in domain objects.
- For complex business domains with rich behavior.
