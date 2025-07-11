# Unit of Work Pattern

The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates the writing out of changes and the resolution of concurrency problems.

## Structure

- **EntityA**: Example entity A.
- **EntityB**: Example entity B.
- **UnitOfWork**: Manages transactions and coordinates changes for A and B.

## Example

```csharp
var uow = new UnitOfWork();
uow.AddA(new EntityA { Name = "A1" });
uow.AddB(new EntityB { Name = "B1" });
uow.Commit(); // Output: Transacción confirmada
uow.Rollback(); // Output: Transacción revertida
```

## When to use

- When you need to coordinate the writing of changes and manage transactions.
- To ensure atomicity and consistency in data operations.
