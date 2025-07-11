# Composite Pattern

The Composite pattern composes objects into tree structures to represent part-whole hierarchies. Composite lets clients treat individual objects and compositions of objects uniformly.

## Structure

- **Component**: Declares the interface for objects in the composition.
- **Leaf**: Represents leaf objects in the composition.
- **Composite**: Defines behavior for components having children.

## Example

```csharp
var leaf = new Leaf();
var composite = new Composite();
composite.Add(leaf);
composite.Operation(); // Output: Composite\nHoja
```

## When to use

- When you want to represent part-whole hierarchies of objects.
- When clients should be able to treat individual objects and compositions uniformly.
