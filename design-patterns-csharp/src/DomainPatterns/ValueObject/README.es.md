# Patrón Value Object

El patrón Value Object representa un objeto definido por sus atributos en lugar de una identidad única.

## Estructura

- **Money**: Value object de ejemplo con igualdad basada en el valor.

## Ejemplo

```csharp
var m1 = new Money(10, "USD");
var m2 = new Money(10, "USD");
Console.WriteLine(m1.Equals(m2)); // Output: True
```

## Cuándo usarlo

- Cuando necesitas objetos intercambiables por su valor.
- Para modelar conceptos que no requieren identidad.
