# Patrón AAA (Arrange, Act, Assert)

El patrón AAA es una estructura común para escribir tests unitarios, dividiendo la prueba en tres secciones claras: Arrange, Act y Assert.

## Estructura

- **Arrange**: Configura los objetos de prueba y prepara los requisitos previos.
- **Act**: Ejecuta el código bajo prueba.
- **Assert**: Verifica el resultado.

## Ejemplo

```csharp
var calc = new Calculator(); // Arrange
var result = calc.Add(2, 3); // Act
if (result == 5) Console.WriteLine("Test passed"); // Assert
```

## Cuándo usarlo

- Para tests unitarios claros y mantenibles.
- Para separar la preparación, ejecución y verificación de la prueba.
