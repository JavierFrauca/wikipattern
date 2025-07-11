# Chain of Responsibility Pattern

The Chain of Responsibility pattern allows a request to pass through a chain of handlers. Each handler decides either to process the request or to pass it to the next handler in the chain. This pattern decouples the sender of a request from its receivers, giving more flexibility in assigning responsibilities to objects.

## Structure
- **Handler**: Declares an interface for handling requests and optionally holds a reference to the next handler.
- **ConcreteHandler**: Handles requests it is responsible for; otherwise, it forwards the request to the next handler.

## Example
```csharp
var a = new ConcreteHandlerA();
var b = new ConcreteHandlerB();
a.SetNext(b);
a.Handle("A"); // Output: Handled by A
a.Handle("B"); // Output: Handled by B
```

## When to use
- When more than one object may handle a request and the handler isn't known a priori.
- When you want to issue a request to one of several objects without specifying the receiver explicitly.
- When the set of handlers should be specified dynamically.
