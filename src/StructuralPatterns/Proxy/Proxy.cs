using System;

// Sujeto
public interface ISubject
{
    string Request();
}

// Sujeto real
public class RealSubject : ISubject
{
    public string Request() => "Solicitud real";
}

// Proxy
public class Proxy : ISubject
{
    private RealSubject _realSubject;
    public string Request()
    {
        if (_realSubject == null)
            _realSubject = new RealSubject();
        return $"Proxy: {_realSubject.Request()}";
    }
}

// Ejemplo de uso
// ISubject proxy = new Proxy();
// Console.WriteLine(proxy.Request()); // Output: Proxy: Solicitud real
