# Patrón Specification

El patrón Specification permite encapsular reglas de negocio y reutilizarlas combinando especificaciones simples en otras más complejas.

## Estructura

- **ISpecification**: Interfaz para reglas de negocio.
- **EvenNumberSpecification**: Implementación de ejemplo.

## Ejemplo

```csharp
var spec = new EvenNumberSpecification();
Console.WriteLine(spec.IsSatisfiedBy(2)); // Output: True
```

## Cuándo usarlo

- Cuando quieres encapsular y reutilizar reglas de negocio.
- Para combinar reglas simples en especificaciones complejas.
