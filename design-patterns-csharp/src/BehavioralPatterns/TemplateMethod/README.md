# Template Method Pattern

The Template Method pattern defines the skeleton of an algorithm in a method, deferring some steps to subclasses. It lets subclasses redefine certain steps of an algorithm without changing its structure.

## Structure

- **AbstractClass**: Defines abstract primitive operations that must be implemented by subclasses and a template method defining the algorithm's structure.
- **ConcreteClass**: Implements the primitive operations.

## Example

```csharp
var obj = new ConcreteClass();
obj.TemplateMethod(); // Output: Paso 1\nPaso 2
```

## When to use

- To implement the invariant parts of an algorithm once and let subclasses implement the behavior that can vary.
- To control the extension points of an algorithm.
