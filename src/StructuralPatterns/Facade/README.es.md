# Patrón Facade

El patrón Facade proporciona una interfaz unificada a un conjunto de interfaces en un subsistema. Facade define una interfaz de más alto nivel que hace que el subsistema sea más fácil de usar.

## Estructura

- **Facade**: Sabe qué clases del subsistema son responsables de una solicitud y delega las solicitudes del cliente a los objetos apropiados.
- **Clases del subsistema**: Implementan la funcionalidad del subsistema.

## Ejemplo

```csharp
var facade = new Facade();
facade.Operation(); // Output: Operación A\nOperación B
```

## Cuándo usarlo

- Para proporcionar una interfaz simple a un subsistema complejo.
- Para desacoplar un subsistema de sus clientes y de otros subsistemas.
