# Patrón Cadena de Responsabilidad

El patrón Cadena de Responsabilidad permite que una solicitud pase a través de una cadena de manejadores. Cada manejador decide si procesa la solicitud o la pasa al siguiente en la cadena. Este patrón desacopla el emisor de la solicitud de sus receptores, dando flexibilidad para asignar responsabilidades.

## Estructura
- **Handler**: Declara una interfaz para manejar solicitudes y opcionalmente mantiene una referencia al siguiente manejador.
- **ConcreteHandler**: Procesa las solicitudes de las que es responsable; de lo contrario, las reenvía al siguiente manejador.

## Ejemplo
```csharp
var a = new ConcreteHandlerA();
var b = new ConcreteHandlerB();
a.SetNext(b);
a.Handle("A"); // Output: Handled by A
a.Handle("B"); // Output: Handled by B
```

## Cuándo usarlo
- Cuando más de un objeto puede manejar una solicitud y el manejador no se conoce a priori.
- Cuando se quiere enviar una solicitud a uno de varios objetos sin especificar explícitamente el receptor.
- Cuando el conjunto de manejadores debe especificarse dinámicamente.
