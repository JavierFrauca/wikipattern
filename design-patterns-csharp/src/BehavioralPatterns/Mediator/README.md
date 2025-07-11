# Mediator Pattern

The Mediator pattern defines an object that encapsulates how a set of objects interact. This pattern promotes loose coupling by keeping objects from referring to each other explicitly, and it lets you vary their interaction independently.

## Structure

- **Mediator**: Defines an interface for communicating with Colleague objects.
- **ConcreteMediator**: Implements cooperative behavior by coordinating Colleague objects.
- **Colleague**: Each Colleague class knows its Mediator object.

## Example

```csharp
var dialog = new Dialog();
var button = new Button(dialog);
button.Click(); // Output: Button clicked, dialog notified
```

## When to use

- To reduce coupling between communicating objects.
- When a set of objects communicate in well-defined but complex ways.
