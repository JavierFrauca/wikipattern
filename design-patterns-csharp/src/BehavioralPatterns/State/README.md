# State Pattern

The State pattern allows an object to alter its behavior when its internal state changes. The object will appear to change its class.

## Structure

- **State**: Defines an interface for encapsulating the behavior associated with a particular state.
- **ConcreteState**: Implements behavior associated with a state of the context.
- **Context**: Maintains an instance of a ConcreteState subclass that defines the current state.

## Example

```csharp
var context = new Context(new StateA());
context.Request(); // Output: Estado A
context.Request(); // Output: Estado B
```

## When to use

- When an object's behavior depends on its state and it must change its behavior at runtime.
- When operations have large, multipart conditional statements that depend on the object's state.
