using System;

// Prototipo base
public abstract class Prototype
{
    public string Name { get; set; }
    public abstract Prototype Clone();
}

// Prototipo concreto
public class ConcretePrototype : Prototype
{
    public override Prototype Clone()
    {
        return (Prototype)this.MemberwiseClone();
    }
}

// Ejemplo de uso
// var original = new ConcretePrototype { Name = "Original" };
// var copia = original.Clone();
// Console.WriteLine(copia.Name); // Output: Original
