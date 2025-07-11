# Abstract Factory

## Description
The Abstract Factory pattern provides an interface for creating families of related or dependent objects without specifying their concrete classes.

## When to use
- When your code needs to work with various families of related products.
- To enforce consistency among products.

## When NOT to use
- When products do not need to be related.
- When only one product type is needed.

## Implementation
```csharp
// Abstract Product A
public interface IButton { void Paint(); }
// Abstract Product B
public interface ICheckbox { void Paint(); }

// Concrete Product A1
public class WinButton : IButton { public void Paint() => Console.WriteLine("Windows Button"); }
// Concrete Product B1
public class WinCheckbox : ICheckbox { public void Paint() => Console.WriteLine("Windows Checkbox"); }

// Abstract Factory
public interface IGUIFactory {
    IButton CreateButton();
    ICheckbox CreateCheckbox();
}

// Concrete Factory
public class WinFactory : IGUIFactory {
    public IButton CreateButton() => new WinButton();
    public ICheckbox CreateCheckbox() => new WinCheckbox();
}
```

## Practical example
```csharp
IGUIFactory factory = new WinFactory();
IButton button = factory.CreateButton();
button.Paint(); // Output: Windows Button
```

## Advantages
- Ensures consistency among products
- Supports families of related products

## Disadvantages
- Can be complex to implement
- Difficult to add new product families

## Related patterns
- Factory Method
- Builder
