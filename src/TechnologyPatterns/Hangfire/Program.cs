using System;
using System.Threading.Tasks;

// ============================================================================
// HANGFIRE BACKGROUND JOBS - PROGRAMA PRINCIPAL
// ============================================================================

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("⚡ HANGFIRE BACKGROUND JOBS DEMO");
        Console.WriteLine("===============================");
        Console.WriteLine();

        try
        {
            await BackgroundJobDemo.RunDemo();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine("\nPresiona cualquier tecla para salir...");
        Console.ReadKey();
    }
}
