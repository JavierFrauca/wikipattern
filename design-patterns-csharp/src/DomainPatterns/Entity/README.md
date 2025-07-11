# Entity Pattern

The Entity pattern represents an object with a distinct identity that runs through time and different states.

## Structure

- **Entity**: Has an identity and equality based on that identity.

## Example

```csharp
var e1 = new Entity { Id = 1 };
var e2 = new Entity { Id = 1 };
Console.WriteLine(e1.Equals(e2)); // Output: True
```

## When to use

- When you need to model objects with a unique identity.
- To distinguish objects by identity rather than attributes.
