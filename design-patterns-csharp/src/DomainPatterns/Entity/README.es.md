# Patrón Entity

El patrón Entity representa un objeto con una identidad distinta que persiste a través del tiempo y diferentes estados.

## Estructura

- **Entity**: Tiene una identidad y la igualdad se basa en esa identidad.

## Ejemplo

```csharp
var e1 = new Entity { Id = 1 };
var e2 = new Entity { Id = 1 };
Console.WriteLine(e1.Equals(e2)); // Output: True
```

## Cuándo usarlo

- Cuando necesitas modelar objetos con identidad única.
- Para distinguir objetos por identidad en lugar de atributos.
