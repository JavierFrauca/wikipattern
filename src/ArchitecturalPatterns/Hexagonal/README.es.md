# Arquitectura Hexagonal (Ports and Adapters)

La Arquitectura Hexagonal, también conocida como Ports and Adapters, estructura una aplicación para que la lógica central esté aislada de las preocupaciones externas (como bases de datos o UI) mediante puertos y adaptadores.

## Estructura

- **Port**: Define una interfaz para la comunicación.
- **Adapter**: Implementa el puerto para interactuar con sistemas externos.
- **Application**: Usa el puerto para toda comunicación externa.

## Ejemplo

```csharp
var app = new Application(new Adapter());
app.Run(); // Output: Sending: Hello Hexagonal
```

## Cuándo usarlo

- Cuando quieres aislar la lógica de negocio de los sistemas externos.
- Para facilitar las pruebas y la adaptación a nuevas tecnologías.
