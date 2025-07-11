# Patrón Visitor

El patrón Visitor permite definir nuevas operaciones sobre objetos sin modificar sus clases. Separa el algoritmo de la estructura de objetos sobre la que opera.

## Estructura

- **Visitor**: Declara una operación de visita para cada tipo de elemento.
- **ConcreteVisitor**: Implementa cada operación declarada por Visitor.
- **Element**: Define un método Accept que recibe un visitante.
- **ConcreteElement**: Implementa el método Accept.

## Ejemplo

```csharp
var element = new ConcreteElement();
var visitor = new ConcreteVisitor();
element.Accept(visitor); // Output: Visitando elemento
```

## Cuándo usarlo

- Cuando necesitas realizar operaciones sobre un conjunto de objetos con diferentes interfaces.
- Cuando la estructura de objetos cambia poco, pero las operaciones cambian frecuentemente.
