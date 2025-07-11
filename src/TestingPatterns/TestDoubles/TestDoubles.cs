using System;

// Test Double Pattern
public interface IService
{
    int GetValue();
}

// Implementación real
public class RealService : IService
{
    public int GetValue() => 42;
}

// Doble de prueba
public class FakeService : IService
{
    public int GetValue() => 1;
}

// Ejemplo de uso
// IService service = new FakeService();
// Console.WriteLine(service.GetValue()); // Output: 1
