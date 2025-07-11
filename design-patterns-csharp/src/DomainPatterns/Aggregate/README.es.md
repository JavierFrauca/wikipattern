# Patrón Aggregate

El patrón Aggregate define un conjunto de objetos de dominio que pueden ser tratados como una sola unidad. Un agregado tiene una raíz y un límite.

## Estructura

- **Aggregate**: Entidad raíz (por ejemplo, OrderAggregate) y sus objetos relacionados.

## Ejemplo

```csharp
var agg = new OrderAggregate();
agg.AddOrder(new Order { Id = 1 });
Console.WriteLine(agg.Order.Id); // Output: 1
```

## Cuándo usarlo

- Cuando quieres aplicar reglas de consistencia a un conjunto de objetos relacionados.
- Para definir límites transaccionales en el modelo de dominio.
