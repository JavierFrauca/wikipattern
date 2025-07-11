# Patrón Mediator

El patrón Mediator define un objeto que encapsula cómo interactúan un conjunto de objetos. Promueve el bajo acoplamiento evitando que los objetos se refieran entre sí explícitamente y permite variar su interacción independientemente.

## Estructura

- **Mediator**: Define una interfaz para la comunicación con los objetos Colleague.
- **ConcreteMediator**: Implementa el comportamiento cooperativo coordinando los objetos Colleague.
- **Colleague**: Cada clase Colleague conoce su objeto Mediator.

## Ejemplo

```csharp
var dialog = new Dialog();
var button = new Button(dialog);
button.Click(); // Output: Button clicked, dialog notified
```

## Cuándo usarlo

- Para reducir el acoplamiento entre objetos que se comunican.
- Cuando un conjunto de objetos se comunican de formas bien definidas pero complejas.
