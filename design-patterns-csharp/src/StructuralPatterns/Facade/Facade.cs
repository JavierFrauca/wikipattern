using System;

// Fachada
public class SubsystemA
{
    public void OperationA() => Console.WriteLine("Operación A");
}

public class SubsystemB
{
    public void OperationB() => Console.WriteLine("Operación B");
}

public class Facade
{
    private SubsystemA _a = new SubsystemA();
    private SubsystemB _b = new SubsystemB();
    public void Operation()
    {
        _a.OperationA();
        _b.OperationB();
    }
}

// Ejemplo de uso
// var facade = new Facade();
// facade.Operation(); // Output: Operación A\nOperación B
