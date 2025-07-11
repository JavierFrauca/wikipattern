using System;

// Hexagonal Architecture (Ports and Adapters)
public interface IPort
{
    void Send(string message);
}

public class Adapter : IPort
{
    public void Send(string message) => Console.WriteLine($"Sending: {message}");
}

public class Application
{
    private readonly IPort _port;
    public Application(IPort port) => _port = port;
    public void Run() => _port.Send("Hello Hexagonal");
}

// Example usage
// var app = new Application(new Adapter());
// app.Run(); // Output: Sending: Hello Hexagonal
