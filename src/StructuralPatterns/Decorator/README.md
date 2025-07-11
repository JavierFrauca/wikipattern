# Decorator Pattern

The Decorator pattern attaches additional responsibilities to an object dynamically. Decorators provide a flexible alternative to subclassing for extending functionality.

## Structure

- **Component**: Defines the interface for objects that can have responsibilities added to them dynamically.
- **ConcreteComponent**: Defines an object to which additional responsibilities can be attached.
- **Decorator**: Maintains a reference to a Component object and defines an interface that conforms to Component's interface.
- **ConcreteDecorator**: Adds responsibilities to the component.

## Example

```csharp
IComponent component = new ConcreteComponent();
component = new ConcreteDecorator(component);
Console.WriteLine(component.Operation()); // Output: Decorado(Componente)
```

## When to use

- To add responsibilities to individual objects dynamically and transparently.
- For responsibilities that can be withdrawn.
