# Patrón Thread Pool

El patrón Thread Pool gestiona un conjunto de hilos de trabajo para realizar tareas, mejorando el rendimiento y la gestión de recursos al reutilizar los hilos.

## Estructura

- **ThreadPool**: Gestiona un conjunto de hilos reutilizables.
- **Task**: Elementos de trabajo que ejecuta el pool.

## Ejemplo

```csharp
var pool = new ThreadPoolExample();
pool.QueueWork(); // Output: Trabajo en el ThreadPool
```

## Cuándo usarlo

- Cuando necesitas ejecutar muchas tareas de corta duración.
- Para evitar el coste de crear y destruir hilos repetidamente.
