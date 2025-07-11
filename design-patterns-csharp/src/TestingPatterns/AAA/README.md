# AAA Pattern (Arrange, Act, Assert)

The AAA pattern is a common structure for writing unit tests, dividing the test into three clear sections: Arrange, Act, and Assert.

## Structure

- **Arrange**: Set up test objects and prepare prerequisites.
- **Act**: Execute the code under test.
- **Assert**: Verify the result.

## Example

```csharp
var calc = new Calculator(); // Arrange
var result = calc.Add(2, 3); // Act
if (result == 5) Console.WriteLine("Test passed"); // Assert
```

## When to use

- For clear and maintainable unit tests.
- To separate test setup, execution, and verification.
