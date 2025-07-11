# Método Fábrica / Factory Method

## Descripción
El Método Fábrica es un patrón creacional que proporciona una interfaz para crear objetos en una superclase, pero permite que las subclases alteren el tipo de objetos que se crearán.

## Cuándo usar
- Cuando una clase no puede anticipar la clase de objetos que debe crear.
- Para delegar la responsabilidad de la creación a subclases.

## Cuándo NO usar
- Cuando la creación de objetos es simple y no requiere lógica adicional.
- Cuando no necesitas variar la clase del producto.

## Implementación

```csharp
// Interfaz Producto
public interface ITransporte
{
    void Entregar();
}

// Productos Concretos
public class Camion : ITransporte
{
    public void Entregar() => Console.WriteLine("Entregar por tierra en una caja");
}
public class Barco : ITransporte
{
    public void Entregar() => Console.WriteLine("Entregar por mar en un contenedor");
}

// Request para la fábrica
public class SolicitudTransporte
{
    public string Tipo { get; set; } // "tierra" o "mar"
}

// Factory Command
public static class TransportFactory
{
    public static ITransporte CrearTransporte(SolicitudTransporte request)
    {
        return request.Tipo switch
        {
            "tierra" => new Camion(),
            "mar" => new Barco(),
            _ => throw new ArgumentException("Tipo de transporte no soportado")
        };
    }
}
```

## Ejemplo práctico


```csharp
var request = new SolicitudTransporte { Tipo = "mar" };
ITransporte transporte = TransportFactory.CrearTransporte(request);
transporte.Entregar(); // Salida: Entregar por mar en un contenedor

request = new SolicitudTransporte { Tipo = "tierra" };
transporte = TransportFactory.CrearTransporte(request);
transporte.Entregar(); // Salida: Entregar por tierra en una caja
```

## Ventajas
- Promueve la reutilización y flexibilidad del código
- Favorece el Principio Abierto/Cerrado

## Desventajas
- Puede introducir clases y complejidad extra

## Patrones relacionados
- Abstract Factory
- Prototype
