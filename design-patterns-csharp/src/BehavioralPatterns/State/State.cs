using System;

// Estado
public interface IState
{
    void Handle(Context context);
}

// Estado concreto
public class StateA : IState
{
    public void Handle(Context context)
    {
        Console.WriteLine("Estado A");
        context.State = new StateB();
    }
}

public class StateB : IState
{
    public void Handle(Context context)
    {
        Console.WriteLine("Estado B");
        context.State = new StateA();
    }
}

// Contexto
public class Context
{
    public IState State { get; set; }
    public Context(IState state) => State = state;
    public void Request() => State.Handle(this);
}

// Ejemplo de uso
// var context = new Context(new StateA());
// context.Request(); // Output: Estado A
// context.Request(); // Output: Estado B
