# Thread Pool Pattern

The Thread Pool pattern manages a pool of worker threads to perform tasks, improving performance and resource management by reusing threads.

## Structure

- **ThreadPool**: Manages a set of reusable threads.
- **Task**: Work items to be executed by the pool.

## Example

```csharp
var pool = new ThreadPoolExample();
pool.QueueWork(); // Output: Trabajo en el ThreadPool
```

## When to use

- When you need to execute many short-lived tasks.
- To avoid the overhead of creating and destroying threads repeatedly.
