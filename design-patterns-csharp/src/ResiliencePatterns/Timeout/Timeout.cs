using System;
using System.Threading;

// Timeout Pattern
public class TimeoutPattern
{
    public bool Execute(Action action, int milliseconds)
    {
        var thread = new Thread(() => action());
        thread.Start();
        bool finished = thread.Join(milliseconds);
        if (!finished)
        {
            thread.Abort();
            Console.WriteLine("Timeout alcanzado");
        }
        return finished;
    }
}

// Ejemplo de uso
// var timeout = new TimeoutPattern();
// timeout.Execute(() => Thread.Sleep(2000), 1000); // Output: Timeout alcanzado
