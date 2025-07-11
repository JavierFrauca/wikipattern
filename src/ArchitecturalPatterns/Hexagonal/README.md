# Hexagonal Architecture (Ports and Adapters)

Hexagonal Architecture, also known as Ports and Adapters, structures an application so that the core logic is isolated from external concerns (like databases or UI) via ports and adapters.

## Structure

- **Port**: Defines an interface for communication.
- **Adapter**: Implements the port to interact with external systems.
- **Application**: Uses the port for all external communication.

## Example

```csharp
var app = new Application(new Adapter());
app.Run(); // Output: Sending: Hello Hexagonal
```

## When to use

- When you want to isolate business logic from external systems.
- To make the application easier to test and adapt to new technologies.
