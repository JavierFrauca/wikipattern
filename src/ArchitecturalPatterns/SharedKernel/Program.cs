using System;
using System.Threading.Tasks;

// ============================================================================
// SHARED KERNEL - PROGRAMA PRINCIPAL
// ============================================================================

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🏗️ SHARED KERNEL / MICROKERNEL PATTERN DEMO");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        try
        {
            await SharedKernelDemo.RunDemo();
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
