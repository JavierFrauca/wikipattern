# Patrón Dependency Injection

El patrón Dependency Injection permite que una clase reciba sus dependencias desde fuentes externas en lugar de crearlas por sí misma, promoviendo bajo acoplamiento y facilitando las pruebas.

## Estructura

- **Service**: La dependencia a inyectar.
- **Client**: La clase que depende del servicio.
- **Injector**: Proporciona el servicio al cliente.

## Ejemplo

```csharp
var service = new Service();
var client = new Client(service);
client.Start(); // Output: Service Called
```

## Cuándo usarlo

- Cuando quieres desacoplar las clases de sus dependencias.
- Para facilitar las pruebas y el mantenimiento.
