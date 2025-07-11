# Patrón Domain Model

El patrón Domain Model organiza la lógica de negocio y los datos como un modelo de objetos rico, encapsulando estado y comportamiento.

## Estructura

- **Product**: Entidad de dominio de ejemplo con lógica de negocio.

## Ejemplo

```csharp
var product = new Product { Name = "Libro", Price = 100 };
product.ApplyDiscount(0.1m);
Console.WriteLine(product.Price); // Output: 90
```

## Cuándo usarlo

- Cuando quieres encapsular reglas y lógica de negocio en objetos de dominio.
- Para dominios de negocio complejos con comportamiento rico.
