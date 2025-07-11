using System;

// MVC Pattern
public class Model
{
    public string Data { get; set; }
}

public class View
{
    public void Render(string data) => Console.WriteLine($"View: {data}");
}

public class Controller
{
    private readonly Model _model;
    private readonly View _view;
    public Controller(Model model, View view)
    {
        _model = model;
        _view = view;
    }
    public void UpdateView()
    {
        _view.Render(_model.Data);
    }
}

// Example usage
// var model = new Model { Data = "Hello MVC" };
// var view = new View();
// var controller = new Controller(model, view);
// controller.UpdateView(); // Output: View: Hello MVC
