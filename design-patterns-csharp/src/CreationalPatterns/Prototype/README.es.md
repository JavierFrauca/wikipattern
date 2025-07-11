# Patrón Prototype

El patrón Prototype crea nuevos objetos copiando un objeto existente, conocido como prototipo. Se utiliza cuando el costo de crear un nuevo objeto es más alto que copiar uno existente.

## Estructura

- **Prototype**: Declara una interfaz para clonarse a sí mismo.
- **ConcretePrototype**: Implementa la operación de clonación.

## Ejemplo

```csharp
var original = new ConcretePrototype { Name = "Original" };
var copia = original.Clone();
Console.WriteLine(copia.Name); // Output: Original
```

## Cuándo usarlo

- Cuando las clases a instanciar se especifican en tiempo de ejecución.
- Para evitar construir una jerarquía de clases factoría.
- Cuando el costo de crear una nueva instancia es mayor que copiar una existente.
