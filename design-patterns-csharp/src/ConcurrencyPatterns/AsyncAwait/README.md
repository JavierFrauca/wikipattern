# Async/Await Pattern

The Async/Await pattern simplifies asynchronous programming by allowing you to write code that looks synchronous but executes asynchronously.

## Structure

- **Async method**: Uses the async modifier and contains await expressions.
- **Awaitable**: An object that can be awaited, such as a Task.

## Example

```csharp
var worker = new AsyncWorker();
await worker.DoWorkAsync(); // Output: Trabajo asíncrono completado
```

## When to use

- When you need to perform non-blocking operations.
- To improve responsiveness in applications.
