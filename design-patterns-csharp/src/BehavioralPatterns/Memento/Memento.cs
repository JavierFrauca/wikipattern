using System;

// Originador
public class Originator
{
    public string State { get; set; }
    public Memento Save() => new Memento(State);
    public void Restore(Memento memento) => State = memento.State;
}

// Memento
public class Memento
{
    public string State { get; }
    public Memento(string state) => State = state;
}

// Ejemplo de uso
// var originator = new Originator { State = "A" };
// var memento = originator.Save();
// originator.State = "B";
// originator.Restore(memento);
// Console.WriteLine(originator.State); // Output: A
