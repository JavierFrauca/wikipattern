using System;
using System.Threading.Tasks;

// Pipeline Step
public interface IStep
{
    Task ExecuteAsync();
}

// Step concreto
public class StepA : IStep
{
    public async Task ExecuteAsync()
    {
        await Task.Delay(50);
        Console.WriteLine("Step A ejecutado");
    }
}

public class StepB : IStep
{
    public async Task ExecuteAsync()
    {
        await Task.Delay(50);
        Console.WriteLine("Step B ejecutado");
    }
}

// Pipeline
public class Pipeline
{
    private IStep[] _steps;
    public Pipeline(params IStep[] steps) => _steps = steps;
    public async Task RunAsync()
    {
        foreach (var step in _steps)
            await step.ExecuteAsync();
    }
}

// Ejemplo de uso
// var pipeline = new Pipeline(new StepA(), new StepB());
// await pipeline.RunAsync(); // Output: Step A ejecutado\nStep B ejecutado
