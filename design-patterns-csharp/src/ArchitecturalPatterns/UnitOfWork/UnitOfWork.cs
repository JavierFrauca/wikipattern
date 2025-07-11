using System;

// Unit of Work Pattern
// Entidad de ejemplo
public class EntityA
{
    public string Name { get; set; }
}

public class EntityB
{
    public string Name { get; set; }
}

// Unit of Work
public class UnitOfWork
{
    public void AddA(EntityA a) => Console.WriteLine($"A agregado: {a.Name}");
    public void AddB(EntityB b) => Console.WriteLine($"B agregado: {b.Name}");
    public void Commit() => Console.WriteLine("Transacción confirmada");
    public void Rollback() => Console.WriteLine("Transacción revertida");
}

// Ejemplo de uso
// var uow = new UnitOfWork();
// uow.AddA(new EntityA { Name = "A1" });
// uow.AddB(new EntityB { Name = "B1" });
// uow.Commit(); // Output: Transacción confirmada
// uow.Rollback(); // Output: Transacción revertida
