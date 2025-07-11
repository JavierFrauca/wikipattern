# TDD Pattern (Test Driven Development)

The TDD pattern is a software development process where tests are written before the code that makes them pass.

## Structure

- **Test**: Written before implementation.
- **Code**: Written to make the test pass.

## Example

```csharp
// Test: assert Add(2, 3) == 5
var calc = new Calculator();
if (calc.Add(2, 3) == 5) Console.WriteLine("Test passed");
```

## When to use

- For test-first development and high code quality.
- To ensure code meets requirements from the start.
