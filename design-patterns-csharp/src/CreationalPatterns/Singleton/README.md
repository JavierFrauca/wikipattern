# Singleton Pattern

The Singleton pattern ensures a class has only one instance and provides a global point of access to it.

## Structure

- **Singleton**: Defines a static method for accessing its unique instance.

## Example

```csharp
Singleton.Instance.Data = "Hello";
Console.WriteLine(Singleton.Instance.Data); // Output: Hello
```

## When to use

- When exactly one instance of a class is needed.
- When a global point of access to the instance is required.
