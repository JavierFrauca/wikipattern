# Patrón Observer

El patrón Observer define una dependencia uno-a-muchos entre objetos, de modo que cuando uno cambia de estado, todos sus dependientes son notificados y actualizados automáticamente.

## Estructura

- **Subject**: Conoce a sus observadores y proporciona una interfaz para agregarlos o quitarlos.
- **Observer**: Define una interfaz de actualización para los objetos que deben ser notificados de cambios en el sujeto.
- **ConcreteSubject/ConcreteObserver**: Implementan las interfaces de sujeto y observador.

## Ejemplo

```csharp
var subject = new Subject();
var observer = new ConcreteObserver();
subject.Attach(observer);
subject.Notify("Hola"); // Output: Recibido: Hola
```

## Cuándo usarlo

- Cuando una abstracción tiene dos aspectos, uno dependiente del otro.
- Cuando un cambio en un objeto requiere cambiar otros, y no sabes cuántos objetos necesitan ser cambiados.
