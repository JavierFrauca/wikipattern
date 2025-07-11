# Patrón Iterador

El patrón Iterador proporciona una forma de acceder secuencialmente a los elementos de un objeto agregado sin exponer su representación interna.

## Estructura

- **Iterator**: Define una interfaz para acceder y recorrer elementos.
- **ConcreteIterator**: Implementa la interfaz del iterador.
- **Aggregate**: Define una interfaz para crear un objeto iterador.
- **ConcreteAggregate**: Implementa la interfaz del agregado.

## Ejemplo

```csharp
var collection = new NumberCollection(new[] { 1, 2, 3 });
var iterator = collection.GetIterator();
while (iterator.HasNext()) Console.WriteLine(iterator.Next()); // Output: 1 2 3
```

## Cuándo usarlo

- Para recorrer una colección sin exponer su estructura interna.
- Para proporcionar múltiples recorridos sobre objetos colección.
