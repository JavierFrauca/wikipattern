# Patrón Async/Await

El patrón Async/Await simplifica la programación asíncrona permitiendo escribir código que parece síncrono pero se ejecuta de forma asíncrona.

## Estructura

- **Método async**: Usa el modificador async y contiene expresiones await.
- **Awaitable**: Un objeto que puede ser esperado, como un Task.

## Ejemplo

```csharp
var worker = new AsyncWorker();
await worker.DoWorkAsync(); // Output: Trabajo asíncrono completado
```

## Cuándo usarlo

- Cuando necesitas realizar operaciones no bloqueantes.
- Para mejorar la capacidad de respuesta en aplicaciones.
