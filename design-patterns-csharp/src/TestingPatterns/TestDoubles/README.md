# Test Double Pattern

The Test Double pattern uses substitute objects (doubles) in place of real dependencies during testing. Types include fakes, stubs, mocks, and spies.

## Structure

- **IService**: Interface for the dependency.
- **RealService**: Real implementation.
- **FakeService**: Test double implementation.

## Example

```csharp
IService service = new FakeService();
Console.WriteLine(service.GetValue()); // Output: 1
```

## When to use

- When you need to isolate the code under test from external dependencies.
- To simulate different scenarios and behaviors in tests.
