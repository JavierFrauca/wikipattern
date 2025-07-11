# Dependency Injection Pattern

The Dependency Injection pattern allows a class to receive its dependencies from external sources rather than creating them itself, promoting loose coupling and easier testing.

## Structure

- **Service**: The dependency to be injected.
- **Client**: The class that depends on the service.
- **Injector**: Provides the service to the client.

## Example

```csharp
var service = new Service();
var client = new Client(service);
client.Start(); // Output: Service Called
```

## When to use

- When you want to decouple classes from their dependencies.
- To facilitate testing and maintainability.
