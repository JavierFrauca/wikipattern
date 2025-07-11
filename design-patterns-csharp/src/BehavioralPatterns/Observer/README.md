# Observer Pattern

The Observer pattern defines a one-to-many dependency between objects so that when one object changes state, all its dependents are notified and updated automatically.

## Structure

- **Subject**: Knows its observers and provides an interface for attaching and detaching them.
- **Observer**: Defines an updating interface for objects that should be notified of changes in a subject.
- **ConcreteSubject/ConcreteObserver**: Implement the subject and observer interfaces.

## Example

```csharp
var subject = new Subject();
var observer = new ConcreteObserver();
subject.Attach(observer);
subject.Notify("Hola"); // Output: Recibido: Hola
```

## When to use

- When an abstraction has two aspects, one dependent on the other.
- When a change to one object requires changing others, and you don't know how many objects need to be changed.
