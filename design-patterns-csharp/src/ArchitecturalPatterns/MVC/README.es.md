# Patrón MVC

El patrón Modelo-Vista-Controlador (MVC) separa una aplicación en tres componentes principales: Modelo, Vista y Controlador. Esta separación ayuda a gestionar la complejidad y permite el desarrollo, prueba y mantenimiento independientes.

## Estructura

- **Modelo**: Gestiona los datos y la lógica de negocio.
- **Vista**: Muestra los datos al usuario.
- **Controlador**: Maneja la entrada del usuario y actualiza el modelo y la vista.

## Ejemplo

```csharp
var model = new Model { Data = "Hello MVC" };
var view = new View();
var controller = new Controller(model, view);
controller.UpdateView(); // Output: View: Hello MVC
```

## Cuándo usarlo

- Cuando quieres separar responsabilidades en una aplicación de interfaz de usuario.
- Para permitir el desarrollo paralelo de la UI y la lógica de negocio.
