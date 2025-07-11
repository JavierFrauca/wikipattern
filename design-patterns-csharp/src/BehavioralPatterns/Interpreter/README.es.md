# Patrón Intérprete

El patrón Intérprete define una representación para una gramática y un intérprete para procesar sentencias en ese lenguaje. Es útil para gramáticas y expresiones simples.

## Estructura

- **AbstractExpression**: Declara una interfaz para operaciones de interpretación.
- **TerminalExpression/NonTerminalExpression**: Implementan la interpretación para símbolos terminales y no terminales.
- **Context**: Contiene información global para el intérprete.

## Ejemplo

```csharp
var expr = new AddExpression(new NumberExpression(2), new NumberExpression(3));
Console.WriteLine(expr.Interpret()); // Output: 5
```

## Cuándo usarlo

- Cuando la gramática es simple y la eficiencia no es crítica.
- Cuando necesitas interpretar sentencias en un lenguaje.
