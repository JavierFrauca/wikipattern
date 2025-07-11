# Builder Pattern

The Builder pattern separates the construction of a complex object from its representation so that the same construction process can create different representations.

## Structure

- **Builder**: Specifies an abstract interface for creating parts of a Product object.
- **ConcreteBuilder**: Constructs and assembles parts of the product by implementing the Builder interface.
- **Director**: Constructs an object using the Builder interface.
- **Product**: Represents the complex object under construction.

## Example

```csharp
var director = new Director();
var builder = new ConcreteBuilder();
var product = director.Construct(builder);
product.Show(); // Output: PartA: A, PartB: B
```

## When to use

- When the construction process must allow different representations for the object that is constructed.
- When the construction process should be independent of the parts that make up the object and how they're assembled.
