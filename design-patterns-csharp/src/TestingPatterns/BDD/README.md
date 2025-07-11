# BDD Pattern (Behavior Driven Development)

The BDD pattern structures tests around the behavior of the system, using the Given-When-Then format.

## Structure

- **Given**: Set up the initial context.
- **When**: Perform the action under test.
- **Then**: Assert the expected outcome.

## Example

```csharp
var calc = new Calculator(); // Given
var result = calc.Add(2, 3); // When
if (result == 5) Console.WriteLine("Then: resultado correcto"); // Then
```

## When to use

- For tests that describe system behavior in a readable way.
- To facilitate collaboration between developers and non-developers.
