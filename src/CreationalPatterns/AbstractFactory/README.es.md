# Fábrica Abstracta / Abstract Factory

## Descripción
El patrón Fábrica Abstracta proporciona una interfaz para crear familias de objetos relacionados o dependientes sin especificar sus clases concretas.

## Cuándo usar
- Cuando tu código necesita trabajar con varias familias de productos relacionados.
- Para garantizar la consistencia entre productos.

## Cuándo NO usar
- Cuando los productos no necesitan estar relacionados.
- Cuando solo se necesita un tipo de producto.

## Implementación

```csharp
// Producto Abstracto A
public interface IBoton { void Pintar(); }
// Producto Abstracto B
public interface ICheckbox { void Pintar(); }

// Productos Concretos Windows
public class WinBoton : IBoton { public void Pintar() => Console.WriteLine("Botón Windows"); }
public class WinCheckbox : ICheckbox { public void Pintar() => Console.WriteLine("Checkbox Windows"); }

// Productos Concretos Mac
public class MacBoton : IBoton { public void Pintar() => Console.WriteLine("Botón Mac"); }
public class MacCheckbox : ICheckbox { public void Pintar() => Console.WriteLine("Checkbox Mac"); }

// Fábrica Abstracta
public interface IFabricaGUI {
    IBoton CrearBoton();
    ICheckbox CrearCheckbox();
}

// Fábrica Concreta Windows
public class WinFabrica : IFabricaGUI {
    public IBoton CrearBoton() => new WinBoton();
    public ICheckbox CrearCheckbox() => new WinCheckbox();
}
// Fábrica Concreta Mac
public class MacFabrica : IFabricaGUI {
    public IBoton CrearBoton() => new MacBoton();
    public ICheckbox CrearCheckbox() => new MacCheckbox();
}

// Cliente que usa la fábrica adecuada
public static class ClienteGUI
{
    public static void PintarInterfaz(IFabricaGUI fabrica)
    {
        var boton = fabrica.CrearBoton();
        var checkbox = fabrica.CrearCheckbox();
        boton.Pintar();
        checkbox.Pintar();
    }
}
```

## Ejemplo práctico

```csharp
IFabricaGUI fabrica = new WinFabrica();
ClienteGUI.PintarInterfaz(fabrica);
// Salida:
// Botón Windows
// Checkbox Windows

fabrica = new MacFabrica();
ClienteGUI.PintarInterfaz(fabrica);
// Salida:
// Botón Mac
// Checkbox Mac
```

## Ventajas
- Garantiza la consistencia entre productos
- Soporta familias de productos relacionados

## Desventajas
- Puede ser complejo de implementar
- Difícil agregar nuevas familias de productos

## Patrones relacionados
- Factory Method
- Builder
