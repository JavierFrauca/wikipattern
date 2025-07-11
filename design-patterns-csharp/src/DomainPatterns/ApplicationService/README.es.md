# Patrón Application Service

El patrón Application Service define una capa de servicios que coordina operaciones entre múltiples objetos de dominio o agregados, proporcionando una API clara para los casos de uso.

## Estructura

- **ApplicationService**: Proporciona métodos para coordinar la lógica de dominio (por ejemplo, DoWorkA, DoWorkB).

## Ejemplo

```csharp
var service = new ApplicationService();
service.DoWorkA("foo"); // Output: A processed: foo
service.DoWorkB("bar"); // Output: B processed: bar
```

## Cuándo usarlo

- Cuando quieres encapsular lógica de aplicación que no encaja naturalmente en una entidad o value object.
- Para proporcionar una API clara para casos de uso y flujos de trabajo.
