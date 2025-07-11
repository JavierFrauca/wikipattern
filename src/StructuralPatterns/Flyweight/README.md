# Flyweight Pattern

The Flyweight pattern uses sharing to support large numbers of fine-grained objects efficiently.

## Structure

- **Flyweight**: Declares an interface through which flyweights can receive and act on extrinsic state.
- **ConcreteFlyweight**: Implements the Flyweight interface and adds storage for intrinsic state.
- **FlyweightFactory**: Creates and manages flyweight objects.

## Example

```csharp
var factory = new FlyweightFactory();
var flyweight = factory.GetFlyweight("A");
flyweight.Operation("external state"); // Output: Intrinsic: A, Extrinsic: external state
```

## When to use

- When an application uses a large number of objects.
- When storage costs are high due to the quantity of objects.
