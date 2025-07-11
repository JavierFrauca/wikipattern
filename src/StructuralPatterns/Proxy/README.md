# Proxy Pattern

The Proxy pattern provides a surrogate or placeholder for another object to control access to it.

## Structure

- **Subject**: Defines the common interface for RealSubject and Proxy.
- **RealSubject**: Defines the real object that the proxy represents.
- **Proxy**: Maintains a reference to the RealSubject and controls access to it.

## Example

```csharp
ISubject proxy = new Proxy();
Console.WriteLine(proxy.Request()); // Output: Proxy: Solicitud real
```

## When to use

- To control access to an object.
- To add additional functionality when accessing an object.
