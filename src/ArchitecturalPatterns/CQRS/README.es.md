# Patrón CQRS

El patrón Command Query Responsibility Segregation (CQRS) separa las operaciones de lectura y escritura en modelos diferentes, usando comandos para actualizar datos y consultas para leerlos.

## Estructura

- **Command**: Representa una intención de cambiar el estado.
- **Command Handler**: Maneja el comando y actualiza el estado.
- **Query**: Representa una solicitud de datos.
- **Query Handler**: Maneja la consulta y devuelve los datos.

## Ejemplo

```csharp
var cmd = new CreateOrderCommand { Product = "Book" };
new CreateOrderHandler().Handle(cmd); // Output: Order created for Book
var query = new GetOrderQuery { OrderId = 1 };
var result = new GetOrderHandler().Handle(query);
Console.WriteLine(result); // Output: Order 1 details
```

## Cuándo usarlo

- Cuando quieres separar la lógica de lectura y escritura para escalabilidad y mantenibilidad.
- Cuando necesitas optimizar las operaciones de lectura y escritura de forma independiente.
