# Patrón TDD (Test Driven Development)

El patrón TDD es un proceso de desarrollo de software donde las pruebas se escriben antes que el código que las hace pasar.

## Estructura

- **Test**: Se escribe antes de la implementación.
- **Código**: Se escribe para que la prueba pase.

## Ejemplo

```csharp
// Test: assert Add(2, 3) == 5
var calc = new Calculator();
if (calc.Add(2, 3) == 5) Console.WriteLine("Test passed");
```

## Cuándo usarlo

- Para desarrollo guiado por pruebas y alta calidad de código.
- Para asegurar que el código cumple los requisitos desde el principio.
