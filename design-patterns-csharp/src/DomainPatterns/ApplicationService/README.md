# Application Service Pattern

The Application Service pattern defines a service layer that coordinates operations across multiple domain objects or aggregates, providing a clear API for use cases.

## Structure

- **ApplicationService**: Provides methods to coordinate domain logic (e.g., DoWorkA, DoWorkB).

## Example

```csharp
var service = new ApplicationService();
service.DoWorkA("foo"); // Output: A processed: foo
service.DoWorkB("bar"); // Output: B processed: bar
```

## When to use

- When you want to encapsulate application logic that doesn't naturally fit in a domain entity or value object.
- To provide a clear API for use cases and workflows.
