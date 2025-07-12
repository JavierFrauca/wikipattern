using System;
using System.Threading.Tasks;

// ============================================================================
// ORCHESTRATOR PATTERN - PROGRAMA PRINCIPAL
// ============================================================================

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎼 ORCHESTRATOR PATTERN DEMO");
        Console.WriteLine("============================");
        Console.WriteLine();

        try
        {
            await WorkflowOrchestratorDemo.RunDemo();
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
