# Patrón Proxy

El patrón Proxy proporciona un sustituto o representante de otro objeto para controlar el acceso a él.

## Estructura

- **Subject**: Define la interfaz común para RealSubject y Proxy.
- **RealSubject**: Define el objeto real que representa el proxy.
- **Proxy**: Mantiene una referencia al RealSubject y controla el acceso a él.

## Ejemplo

```csharp
ISubject proxy = new Proxy();
Console.WriteLine(proxy.Request()); // Output: Proxy: Solicitud real
```

## Cuándo usarlo

- Para controlar el acceso a un objeto.
- Para añadir funcionalidad adicional al acceder a un objeto.
