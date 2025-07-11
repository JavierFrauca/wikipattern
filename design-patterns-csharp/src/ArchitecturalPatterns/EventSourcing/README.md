# Event Sourcing Pattern

The Event Sourcing pattern stores the state of a system as a sequence of events. Instead of storing just the current state, all changes (events) are persisted, allowing the state to be rebuilt by replaying events.

## Structure

- **Event**: Represents a change in state.
- **Event Store**: Stores all events.

## Example

```csharp
var store = new EventStore();
store.Save(new Event { Name = "Created" });
foreach (var evt in store.GetAll()) Console.WriteLine(evt.Name); // Output: Created
```

## When to use

- When you need a complete audit trail of changes.
- To enable rebuilding state from a history of events.
