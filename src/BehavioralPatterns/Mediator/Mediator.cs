using System;

// Mediador
public interface IMediator
{
    void Notify(object sender, string ev);
}

// Componente base
public class Component
{
    protected IMediator _mediator;
    public Component(IMediator mediator) => _mediator = mediator;
}

// Componente concreto
public class Button : Component
{
    public Button(IMediator mediator) : base(mediator) { }
    public void Click() => _mediator.Notify(this, "click");
}

public class Dialog : IMediator
{
    public void Notify(object sender, string ev)
    {
        if (ev == "click")
            Console.WriteLine("Button clicked, dialog notified");
    }
}

// Ejemplo de uso
// var dialog = new Dialog();
// var button = new Button(dialog);
// button.Click(); // Output: Button clicked, dialog notified
