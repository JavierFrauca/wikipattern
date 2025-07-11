# MVVM Pattern

The Model-View-ViewModel (MVVM) pattern separates the development of the graphical user interface from the business logic or back-end logic. The ViewModel acts as a link between the View and the Model.

## Structure

- **Model**: Manages the data and business logic.
- **View**: Displays data and interacts with the user.
- **ViewModel**: Exposes data and commands from the model to the view.

## Example

```csharp
var vm = new ViewModel { Data = "Hello MVVM" };
var view = new View();
view.Display(vm.Data); // Output: View: Hello MVVM
```

## When to use

- When you want to separate the UI from business logic in applications with data binding.
- To facilitate unit testing of the logic.
