using System;

// Comando
public interface ICommand
{
    void Execute();
}

// Comando concreto
public class HelloCommand : ICommand
{
    public void Execute() => Console.WriteLine("Hello Command");
}

// Invocador
public class Invoker
{
    private ICommand _command;
    public void SetCommand(ICommand command) => _command = command;
    public void Run() => _command.Execute();
}

// Ejemplo de uso
// var invoker = new Invoker();
// invoker.SetCommand(new HelloCommand());
// invoker.Run(); // Output: Hello Command
