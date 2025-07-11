# Aggregate Pattern

The Aggregate pattern defines a cluster of domain objects that can be treated as a single unit. An aggregate has a root and a boundary.

## Structure

- **Aggregate**: Root entity (e.g., OrderAggregate) and its related objects.

## Example

```csharp
var agg = new OrderAggregate();
agg.AddOrder(new Order { Id = 1 });
Console.WriteLine(agg.Order.Id); // Output: 1
```

## When to use

- When you want to enforce consistency rules for a set of related objects.
- To define transactional boundaries in the domain model.
