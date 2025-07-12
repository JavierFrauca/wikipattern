using System;
using System.Threading.Tasks;

namespace EventDrivenDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await EventDrivenDemo.RunDemo();
            
            Console.WriteLine("\nPresiona cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
