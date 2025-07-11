# Patrón Bridge

El patrón Bridge desacopla una abstracción de su implementación, de modo que ambas puedan variar independientemente.

## Estructura

- **Abstraction**: Define la interfaz de la abstracción y mantiene una referencia a un implementador.
- **RefinedAbstraction**: Extiende la interfaz definida por Abstraction.
- **Implementor**: Define la interfaz para las clases de implementación.
- **ConcreteImplementor**: Implementa la interfaz Implementor.

## Ejemplo

```csharp
var impl = new ConcreteImplementorA();
var abs = new RefinedAbstraction(impl);
abs.Operation(); // Output: Implementación A
```

## Cuándo usarlo

- Cuando se quiere evitar una vinculación permanente entre una abstracción y su implementación.
- Cuando tanto las abstracciones como sus implementaciones deben poder extenderse mediante herencia.
