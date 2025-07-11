using System;

// MVVM Pattern
public class ViewModel
{
    public string Data { get; set; }
}

public class View
{
    public void Display(string data) => Console.WriteLine($"View: {data}");
}

// Example usage
// var vm = new ViewModel { Data = "Hello MVVM" };
// var view = new View();
// view.Display(vm.Data); // Output: View: Hello MVVM
