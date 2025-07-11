# Repository Pattern

The Repository pattern mediates between the domain and data mapping layers, acting like an in-memory collection of domain objects. It decouples the business logic from data access logic.

## Structure

- **Repository**: Interface for data access operations.
- **InMemoryRepository**: Example implementation.

## Example

```csharp
IRepository<string> repo = new InMemoryRepository<string>();
repo.Add("data"); // Output: Added: data
```

## When to use

- When you want to decouple business logic from data access logic.
- To provide a simple interface for data operations.
