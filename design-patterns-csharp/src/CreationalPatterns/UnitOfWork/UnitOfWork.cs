using System;

// Unit of Work
public class UnitOfWork
{
    public void Commit() => Console.WriteLine("Transacción confirmada");
    public void Rollback() => Console.WriteLine("Transacción revertida");
}

// Ejemplo de uso
// var uow = new UnitOfWork();
// uow.Commit(); // Output: Transacción confirmada
// uow.Rollback(); // Output: Transacción revertida
