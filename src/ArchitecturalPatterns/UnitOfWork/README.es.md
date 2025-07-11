# Patrón Unit of Work

El patrón Unit of Work mantiene una lista de objetos afectados por una transacción de negocio y coordina la escritura de los cambios y la resolución de problemas de concurrencia.

## Estructura

- **EntityA**: Entidad de ejemplo A.
- **EntityB**: Entidad de ejemplo B.
- **UnitOfWork**: Gestiona transacciones y coordina los cambios de A y B.

## Ejemplo

```csharp
var uow = new UnitOfWork();
uow.AddA(new EntityA { Name = "A1" });
uow.AddB(new EntityB { Name = "B1" });
uow.Commit(); // Output: Transacción confirmada
uow.Rollback(); // Output: Transacción revertida
```

## Cuándo usarlo

- Cuando necesitas coordinar la escritura de cambios y gestionar transacciones.
- Para asegurar la atomicidad y consistencia en las operaciones de datos.
