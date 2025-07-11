# Pipeline Pattern

The Pipeline pattern organizes processing into a sequence of stages, where the output of one stage is the input to the next. Each stage is typically implemented as a separate component.

## Structure

- **Step**: Represents a stage in the pipeline.
- **Pipeline**: Manages the sequence and execution of steps.

## Example

```csharp
var pipeline = new Pipeline(new StepA(), new StepB());
await pipeline.RunAsync(); // Output: Step A ejecutado\nStep B ejecutado
```

## When to use

- When you want to process data in a series of steps.
- To improve modularity and reusability of processing logic.
