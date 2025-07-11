# Domain Events Pattern

The Domain Events pattern captures and communicates significant events that occur within the domain.

## Structure

- **DomainEvent**: Represents a domain event.
- **EventPublisher**: Publishes events to subscribers.

## Example

```csharp
var publisher = new EventPublisher();
publisher.Subscribe(e => Console.WriteLine($"Evento: {e.Name}"));
publisher.Publish(new DomainEvent { Name = "Creado" }); // Output: Evento: Creado
```

## When to use

- When you want to decouple event producers from consumers.
- To track and react to important domain changes.
