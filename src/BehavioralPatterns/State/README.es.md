# Patrón State

El patrón State permite que un objeto modifique su comportamiento cuando cambia su estado interno. El objeto parecerá cambiar de clase.

## Estructura

- **State**: Define una interfaz para encapsular el comportamiento asociado a un estado particular.
- **ConcreteState**: Implementa el comportamiento asociado a un estado del contexto.
- **Context**: Mantiene una instancia de una subclase ConcreteState que define el estado actual.

## Ejemplo

```csharp
var context = new Context(new StateA());
context.Request(); // Output: Estado A
context.Request(); // Output: Estado B
```

## Cuándo usarlo

- Cuando el comportamiento de un objeto depende de su estado y debe cambiar en tiempo de ejecución.
- Cuando las operaciones tienen grandes condicionales que dependen del estado del objeto.
