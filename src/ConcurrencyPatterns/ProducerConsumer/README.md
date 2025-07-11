# Producer-Consumer Pattern

The Producer-Consumer pattern coordinates the work of producers (which generate data) and consumers (which process data), typically using a shared queue.

## Structure

- **Producer**: Generates data and adds it to a queue.
- **Consumer**: Takes data from the queue and processes it.
- **Queue**: Shared buffer between producers and consumers.

## Example

```csharp
var pc = new ProducerConsumer();
Task.Run(() => pc.Produce(42));
Console.WriteLine(pc.Consume()); // Output: 42
```

## When to use

- When you need to decouple the production and consumption of data.
- To balance workloads between producers and consumers.
