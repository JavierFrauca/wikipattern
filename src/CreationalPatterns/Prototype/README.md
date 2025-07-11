# Prototype Pattern

The Prototype pattern creates new objects by copying an existing object, known as the prototype. This pattern is used when the cost of creating a new object is more expensive than copying an existing one.

## Structure

- **Prototype**: Declares an interface for cloning itself.
- **ConcretePrototype**: Implements the operation for cloning itself.

## Example

```csharp
var original = new ConcretePrototype { Name = "Original" };
var copy = original.Clone();
Console.WriteLine(copy.Name); // Output: Original
```

## When to use

- When the classes to instantiate are specified at runtime.
- To avoid building a hierarchy of factory classes.
- When the cost of creating a new instance is more expensive than copying an existing one.
