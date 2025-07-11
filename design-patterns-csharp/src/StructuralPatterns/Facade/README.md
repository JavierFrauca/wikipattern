# Facade Pattern

The Facade pattern provides a unified interface to a set of interfaces in a subsystem. Facade defines a higher-level interface that makes the subsystem easier to use.

## Structure

- **Facade**: Knows which subsystem classes are responsible for a request and delegates client requests to appropriate subsystem objects.
- **Subsystem classes**: Implement subsystem functionality.

## Example

```csharp
var facade = new Facade();
facade.Operation(); // Output: Operación A\nOperación B
```

## When to use

- To provide a simple interface to a complex subsystem.
- To decouple a subsystem from its clients and other subsystems.
