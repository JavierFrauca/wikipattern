using System;
using System.Threading;

// ThreadPool Pattern
public class ThreadPoolExample
{
    public void QueueWork()
    {
        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            Console.WriteLine("Trabajo en el ThreadPool");
        });
    }
}

// Ejemplo de uso
// var pool = new ThreadPoolExample();
// pool.QueueWork(); // Output: Trabajo en el ThreadPool
