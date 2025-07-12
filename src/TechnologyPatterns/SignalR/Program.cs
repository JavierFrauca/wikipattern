using System;
using System.Threading.Tasks;

// ============================================================================
// SIGNALR REAL-TIME COMMUNICATION - PROGRAMA PRINCIPAL
// ============================================================================

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("📡 SIGNALR REAL-TIME COMMUNICATION DEMO");
        Console.WriteLine("======================================");
        Console.WriteLine();

        try
        {
            await RealtimeHubDemo.RunDemo();
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
