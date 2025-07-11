# Patrón Event Sourcing

El patrón Event Sourcing almacena el estado de un sistema como una secuencia de eventos. En lugar de guardar solo el estado actual, se persisten todos los cambios (eventos), permitiendo reconstruir el estado reproduciendo los eventos.

## Estructura

- **Event**: Representa un cambio de estado.
- **Event Store**: Almacena todos los eventos.

## Ejemplo

```csharp
var store = new EventStore();
store.Save(new Event { Name = "Created" });
foreach (var evt in store.GetAll()) Console.WriteLine(evt.Name); // Output: Created
```

## Cuándo usarlo

- Cuando necesitas una auditoría completa de los cambios.
- Para poder reconstruir el estado a partir del historial de eventos.
