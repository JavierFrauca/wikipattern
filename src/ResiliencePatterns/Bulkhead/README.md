# Bulkhead Pattern

The Bulkhead pattern isolates elements of an application into pools so that if one fails, the others will continue to function.

## Structure

- **Bulkhead**: Controls concurrent access to a resource or section.

## Example

```csharp
var bulkhead = new Bulkhead(2);
if (bulkhead.TryEnter()) { /* do work */ bulkhead.Leave(); }
```

## When to use

- To prevent a failure in one part of the system from cascading to others.
- To improve system resilience and fault isolation.
