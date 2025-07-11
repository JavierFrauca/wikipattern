using System;

// Retry Pattern
public class Retry
{
    public void Execute(Action action, int maxAttempts)
    {
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            try
            {
                action();
                return;
            }
            catch
            {
                attempts++;
                if (attempts == maxAttempts)
                    Console.WriteLine("Max reintentos alcanzados");
            }
        }
    }
}

// Ejemplo de uso
// var retry = new Retry();
// retry.Execute(() => throw new Exception(), 2); // Output: Max reintentos alcanzados
