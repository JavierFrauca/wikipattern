# Interpreter Pattern

The Interpreter pattern defines a representation for a grammar and an interpreter to process sentences in the language. It is useful for simple grammars and expressions.

## Structure

- **AbstractExpression**: Declares an interface for interpreting operations.
- **TerminalExpression/NonTerminalExpression**: Implements interpretation for terminal and nonterminal symbols.
- **Context**: Contains information that is global to the interpreter.

## Example

```csharp
var expr = new AddExpression(new NumberExpression(2), new NumberExpression(3));
Console.WriteLine(expr.Interpret()); // Output: 5
```

## When to use

- When the grammar is simple and efficiency is not a concern.
- When you need to interpret sentences in a language.
