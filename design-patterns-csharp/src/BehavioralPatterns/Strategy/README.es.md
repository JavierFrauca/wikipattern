# Patrón Strategy

El patrón Strategy define una familia de algoritmos, los encapsula y los hace intercambiables. Permite que el algoritmo varíe independientemente de los clientes que lo utilizan.

## Estructura

- **Strategy**: Declara una interfaz común para todos los algoritmos soportados.
- **ConcreteStrategy**: Implementa el algoritmo usando la interfaz Strategy.
- **Context**: Mantiene una referencia a un objeto Strategy.

## Ejemplo

```csharp
var context = new Context(new AddStrategy());
Console.WriteLine(context.Execute(2, 3)); // Output: 5
context = new Context(new SubtractStrategy());
Console.WriteLine(context.Execute(5, 2)); // Output: 3
```

## Cuándo usarlo

- Cuando tienes muchas clases relacionadas que solo difieren en su comportamiento.
- Cuando necesitas diferentes variantes de un algoritmo.
