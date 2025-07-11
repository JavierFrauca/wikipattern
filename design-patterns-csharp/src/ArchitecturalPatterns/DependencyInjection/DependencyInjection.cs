using System;

// Dependency Injection Pattern
public interface IService
{
    void Serve();
}

public class Service : IService
{
    public void Serve() => Console.WriteLine("Service Called");
}

public class Client
{
    private readonly IService _service;
    public Client(IService service) => _service = service;
    public void Start() => _service.Serve();
}

// Example usage
// var service = new Service();
// var client = new Client(service);
// client.Start(); // Output: Service Called
