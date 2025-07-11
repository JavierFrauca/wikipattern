# Patrón Command

El patrón Command encapsula una solicitud como un objeto, permitiendo parametrizar clientes con diferentes solicitudes, encolar o registrar solicitudes y soportar operaciones deshacer/rehacer.

## Estructura

- **Command**: Declara una interfaz para ejecutar una operación.
- **ConcreteCommand**: Implementa la interfaz y define la relación entre el receptor y la acción.
- **Invoker**: Solicita al comando que ejecute la operación.
- **Receiver**: Sabe cómo realizar las operaciones asociadas a la solicitud.

## Ejemplo

```csharp
var invoker = new Invoker();
invoker.SetCommand(new HelloCommand());
invoker.Run(); // Output: Hello Command
```

## Cuándo usarlo

- Para parametrizar objetos con operaciones.
- Para soportar operaciones deshacer/rehacer.
- Para encolar, programar o registrar operaciones.
