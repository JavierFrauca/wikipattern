using System;
using System.Collections.Generic;

// Event Sourcing Pattern
public class Event
{
    public string Name { get; set; }
}

public class EventStore
{
    private List<Event> _events = new();
    public void Save(Event evt) => _events.Add(evt);
    public IEnumerable<Event> GetAll() => _events;
}

// Example usage
// var store = new EventStore();
// store.Save(new Event { Name = "Created" });
// foreach (var evt in store.GetAll()) Console.WriteLine(evt.Name); // Output: Created
