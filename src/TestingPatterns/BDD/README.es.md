# Patrón BDD (Behavior Driven Development)

El patrón BDD estructura los tests en torno al comportamiento del sistema, usando el formato Given-When-Then.

## Estructura

- **Given**: Configura el contexto inicial.
- **When**: Realiza la acción bajo prueba.
- **Then**: Verifica el resultado esperado.

## Ejemplo

```csharp
var calc = new Calculator(); // Given
var result = calc.Add(2, 3); // When
if (result == 5) Console.WriteLine("Then: resultado correcto"); // Then
```

## Cuándo usarlo

- Para tests que describen el comportamiento del sistema de forma legible.
- Para facilitar la colaboración entre desarrolladores y no desarrolladores.
