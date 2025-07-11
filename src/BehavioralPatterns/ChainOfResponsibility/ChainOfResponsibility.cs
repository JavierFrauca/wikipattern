using System;

// Handler base
public abstract class Handler
{
    protected Handler Next;
    public void SetNext(Handler next) => Next = next;
    public abstract void Handle(string request);
}

// Handler concreto
public class ConcreteHandlerA : Handler
{
    public override void Handle(string request)
    {
        if (request == "A")
            Console.WriteLine("Handled by A");
        else
            Next?.Handle(request);
    }
}

public class ConcreteHandlerB : Handler
{
    public override void Handle(string request)
    {
        if (request == "B")
            Console.WriteLine("Handled by B");
        else
            Next?.Handle(request);
    }
}

// Ejemplo de uso
// var a = new ConcreteHandlerA();
// var b = new ConcreteHandlerB();
// a.SetNext(b);
// a.Handle("A"); // Output: Handled by A
// a.Handle("B"); // Output: Handled by B
