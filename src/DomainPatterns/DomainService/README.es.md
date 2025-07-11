# Patrón Domain Service

El patrón Domain Service encapsula lógica de dominio que no encaja naturalmente en una sola entidad o value object.

## Estructura

- **DomainService**: Proporciona operaciones de dominio (por ejemplo, IsValid).

## Ejemplo

```csharp
var service = new DomainService();
Console.WriteLine(service.IsValid("abc")); // Output: True
```

## Cuándo usarlo

- Cuando la lógica de dominio abarca varias entidades o value objects.
- Para mantener las entidades y value objects enfocados en sus responsabilidades principales.
