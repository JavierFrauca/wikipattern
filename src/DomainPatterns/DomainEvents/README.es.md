# Patrón Domain Events

El patrón Domain Events captura y comunica eventos significativos que ocurren dentro del dominio.

## Estructura

- **DomainEvent**: Representa un evento de dominio.
- **EventPublisher**: Publica eventos a los suscriptores.

## Ejemplo

```csharp
var publisher = new EventPublisher();
publisher.Subscribe(e => Console.WriteLine($"Evento: {e.Name}"));
publisher.Publish(new DomainEvent { Name = "Creado" }); // Output: Evento: Creado
```

## Cuándo usarlo

- Cuando quieres desacoplar productores y consumidores de eventos.
- Para rastrear y reaccionar a cambios importantes en el dominio.
