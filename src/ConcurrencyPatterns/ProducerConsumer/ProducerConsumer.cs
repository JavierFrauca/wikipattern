using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

// Producer-Consumer Pattern
public class ProducerConsumer
{
    private BlockingCollection<int> _queue = new();
    public void Produce(int item) => _queue.Add(item);
    public int Consume() => _queue.Take();
}

// Ejemplo de uso
// var pc = new ProducerConsumer();
// Task.Run(() => pc.Produce(42));
// Console.WriteLine(pc.Consume()); // Output: 42
