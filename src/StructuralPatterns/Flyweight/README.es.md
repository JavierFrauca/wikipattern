# Patrón Flyweight

El patrón Flyweight utiliza el compartimiento para soportar grandes cantidades de objetos de grano fino de manera eficiente.

## Estructura

- **Flyweight**: Declara una interfaz para que los flyweights reciban y actúen sobre el estado extrínseco.
- **ConcreteFlyweight**: Implementa la interfaz Flyweight y almacena el estado intrínseco.
- **FlyweightFactory**: Crea y gestiona objetos flyweight.

## Ejemplo

```csharp
var factory = new FlyweightFactory();
var flyweight = factory.GetFlyweight("A");
flyweight.Operation("estado externo"); // Output: Intrinsic: A, Extrinsic: estado externo
```

## Cuándo usarlo

- Cuando una aplicación utiliza una gran cantidad de objetos.
- Cuando los costes de almacenamiento son altos debido a la cantidad de objetos.
