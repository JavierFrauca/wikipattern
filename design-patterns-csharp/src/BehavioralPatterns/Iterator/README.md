# Iterator Pattern

The Iterator pattern provides a way to access the elements of an aggregate object sequentially without exposing its underlying representation.

## Structure

- **Iterator**: Defines an interface for accessing and traversing elements.
- **ConcreteIterator**: Implements the iterator interface.
- **Aggregate**: Defines an interface for creating an iterator object.
- **ConcreteAggregate**: Implements the aggregate interface.

## Example

```csharp
var collection = new NumberCollection(new[] { 1, 2, 3 });
var iterator = collection.GetIterator();
while (iterator.HasNext()) Console.WriteLine(iterator.Next()); // Output: 1 2 3
```

## When to use

- To traverse a collection without exposing its internal structure.
- To provide multiple traversals of collection objects.
