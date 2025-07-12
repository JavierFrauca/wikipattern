using System;
using System.Threading.Tasks;

// ============================================================================
// MICROSERVICES EVENT BUS - PROGRAMA PRINCIPAL
// ============================================================================

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎯 DEMOSTRACIONES DE PATRONES MICROSERVICIOS");
        Console.WriteLine("============================================");
        Console.WriteLine();

        try
        {
            while (true)
            {
                Console.WriteLine("Selecciona una demostración:");
                Console.WriteLine("1. 🔄 Microservices Event Bus");
                Console.WriteLine("2. 🏗️ Shared Kernel / Microkernel");
                Console.WriteLine("3. 🎼 Orchestrator Pattern");
                Console.WriteLine("4. ⚡ Hangfire Background Jobs");
                Console.WriteLine("5. 📡 SignalR Real-time Communication");
                Console.WriteLine("0. Salir");
                Console.WriteLine();
                Console.Write("Opción: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await MicroservicesEventBusDemo.RunDemo();
                        break;
                    case "2":
                        await SharedKernelDemo.RunDemo();
                        break;
                    case "3":
                        await WorkflowOrchestratorDemo.RunDemo();
                        break;
                    case "4":
                        await BackgroundJobDemo.RunDemo();
                        break;
                    case "5":
                        await RealtimeHubDemo.RunDemo();
                        break;
                    case "0":
                        Console.WriteLine("¡Hasta luego! 👋");
                        return;
                    default:
                        Console.WriteLine("❌ Opción no válida");
                        break;
                }

                Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                Console.ReadKey();
                Console.Clear();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
