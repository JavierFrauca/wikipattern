# Patrón MVVM

El patrón Modelo-Vista-ViewModel (MVVM) separa el desarrollo de la interfaz gráfica de usuario de la lógica de negocio o lógica de back-end. El ViewModel actúa como enlace entre la Vista y el Modelo.

## Estructura

- **Modelo**: Gestiona los datos y la lógica de negocio.
- **Vista**: Muestra los datos e interactúa con el usuario.
- **ViewModel**: Expone datos y comandos del modelo a la vista.

## Ejemplo

```csharp
var vm = new ViewModel { Data = "Hello MVVM" };
var view = new View();
view.Display(vm.Data); // Output: View: Hello MVVM
```

## Cuándo usarlo

- Cuando quieres separar la UI de la lógica de negocio en aplicaciones con data binding.
- Para facilitar las pruebas unitarias de la lógica.
