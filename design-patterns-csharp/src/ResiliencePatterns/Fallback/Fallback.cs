using System;

// Fallback Pattern
public class Fallback
{
    public void Execute(Action primary, Action fallback)
    {
        try
        {
            primary();
        }
        catch
        {
            fallback();
        }
    }
}

// Ejemplo de uso
// var fb = new Fallback();
// fb.Execute(() => throw new Exception(), () => Console.WriteLine("Fallback ejecutado")); // Output: Fallback ejecutado
