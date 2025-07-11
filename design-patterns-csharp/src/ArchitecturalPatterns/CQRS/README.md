# CQRS Pattern

The Command Query Responsibility Segregation (CQRS) pattern separates read and write operations into different models, using commands to update data and queries to read data.

## Structure

- **Command**: Represents an intent to change the state.
- **Command Handler**: Handles the command and updates the state.
- **Query**: Represents a request for data.
- **Query Handler**: Handles the query and returns data.

## Example

```csharp
var cmd = new CreateOrderCommand { Product = "Book" };
new CreateOrderHandler().Handle(cmd); // Output: Order created for Book
var query = new GetOrderQuery { OrderId = 1 };
var result = new GetOrderHandler().Handle(query);
Console.WriteLine(result); // Output: Order 1 details
```

## When to use

- When you want to separate read and write logic for scalability and maintainability.
- When you need to optimize read and write operations independently.
