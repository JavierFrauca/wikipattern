# Patrón Builder

El patrón Builder separa la construcción de un objeto complejo de su representación, de modo que el mismo proceso de construcción puede crear diferentes representaciones.

## Estructura

- **Builder**: Especifica una interfaz abstracta para crear partes de un objeto Producto.
- **ConcreteBuilder**: Construye y ensambla partes del producto implementando la interfaz Builder.
- **Director**: Construye un objeto usando la interfaz Builder.
- **Product**: Representa el objeto complejo en construcción.

## Ejemplo

```csharp
var director = new Director();
var builder = new ConcreteBuilder();
var product = director.Construct(builder);
product.Show(); // Output: PartA: A, PartB: B
```

## Cuándo usarlo

- Cuando el proceso de construcción debe permitir diferentes representaciones para el objeto construido.
- Cuando el proceso de construcción debe ser independiente de las partes que componen el objeto y de cómo se ensamblan.
