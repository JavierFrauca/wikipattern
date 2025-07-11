# Patrón Decorator

El patrón Decorator añade responsabilidades adicionales a un objeto de manera dinámica. Los decoradores proporcionan una alternativa flexible a la herencia para extender funcionalidades.

## Estructura

- **Component**: Define la interfaz para los objetos que pueden tener responsabilidades añadidas dinámicamente.
- **ConcreteComponent**: Define un objeto al que se le pueden añadir responsabilidades.
- **Decorator**: Mantiene una referencia a un objeto Component y define una interfaz que cumple con la de Component.
- **ConcreteDecorator**: Añade responsabilidades al componente.

## Ejemplo

```csharp
IComponent component = new ConcreteComponent();
component = new ConcreteDecorator(component);
Console.WriteLine(component.Operation()); // Output: Decorado(Componente)
```

## Cuándo usarlo

- Para añadir responsabilidades a objetos individuales de manera dinámica y transparente.
- Para responsabilidades que pueden ser retiradas.
