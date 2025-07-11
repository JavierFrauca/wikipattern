using System;

// Domain Service Pattern
public class DomainService
{
    public bool IsValid(string value) => !string.IsNullOrEmpty(value);
}

// Ejemplo de uso
// var service = new DomainService();
// Console.WriteLine(service.IsValid("abc")); // Output: True
