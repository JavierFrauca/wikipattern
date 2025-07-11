# Command Pattern

The Command pattern encapsulates a request as an object, allowing you to parameterize clients with different requests, queue or log requests, and support undoable operations.

## Structure

- **Command**: Declares an interface for executing an operation.
- **ConcreteCommand**: Implements the command interface and defines the binding between a receiver and an action.
- **Invoker**: Asks the command to carry out the request.
- **Receiver**: Knows how to perform the operations associated with carrying out a request.

## Example

```csharp
var invoker = new Invoker();
invoker.SetCommand(new HelloCommand());
invoker.Run(); // Output: Hello Command
```

## When to use

- To parameterize objects with operations.
- To support undo/redo operations.
- To queue operations, schedule their execution, or log them.
