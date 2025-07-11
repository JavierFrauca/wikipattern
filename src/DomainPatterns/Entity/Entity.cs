using System;

// Entity Pattern
public class Entity
{
    public int Id { get; set; }
    public override bool Equals(object obj) => obj is Entity e && Id == e.Id;
    public override int GetHashCode() => Id.GetHashCode();
}

// Ejemplo de uso
// var e1 = new Entity { Id = 1 };
// var e2 = new Entity { Id = 1 };
// Console.WriteLine(e1.Equals(e2)); // Output: True
