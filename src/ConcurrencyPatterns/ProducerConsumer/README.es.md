# Patrón Producer-Consumer

El patrón Producer-Consumer coordina el trabajo de productores (que generan datos) y consumidores (que procesan datos), normalmente usando una cola compartida.

## Estructura

- **Producer**: Genera datos y los añade a una cola.
- **Consumer**: Toma datos de la cola y los procesa.
- **Queue**: Búfer compartido entre productores y consumidores.

## Ejemplo

```csharp
var pc = new ProducerConsumer();
Task.Run(() => pc.Produce(42));
Console.WriteLine(pc.Consume()); // Output: 42
```

## Cuándo usarlo

- Cuando necesitas desacoplar la producción y el consumo de datos.
- Para balancear la carga de trabajo entre productores y consumidores.
