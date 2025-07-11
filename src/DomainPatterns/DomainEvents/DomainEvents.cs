using System;
using System.Collections.Generic;

// Domain Events Pattern
public class DomainEvent
{
    public string Name { get; set; }
}

public class EventPublisher
{
    private List<Action<DomainEvent>> _subscribers = new();
    public void Subscribe(Action<DomainEvent> handler) => _subscribers.Add(handler);
    public void Publish(DomainEvent domainEvent)
    {
        foreach (var handler in _subscribers)
            handler(domainEvent);
    }
}

// Ejemplo de uso
// var publisher = new EventPublisher();
// publisher.Subscribe(e => Console.WriteLine($"Evento: {e.Name}"));
// publisher.Publish(new DomainEvent { Name = "Creado" }); // Output: Evento: Creado
