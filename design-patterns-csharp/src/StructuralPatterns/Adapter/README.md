# Adapter Pattern

The Adapter pattern allows objects with incompatible interfaces to work together by wrapping its own interface around that of an already existing class.

## Structure

- **Target**: Defines the domain-specific interface used by the client.
- **Adapter**: Adapts the interface of Adaptee to the Target interface.
- **Adaptee**: Defines an existing interface that needs adapting.

## Example

```csharp
var adaptee = new Adaptee();
ITarget target = new Adapter(adaptee);
target.Request(); // Output: Llamada específica del Adaptee
```

## When to use

- When you want to use an existing class, but its interface does not match the one you need.
- To make unrelated classes work together.
