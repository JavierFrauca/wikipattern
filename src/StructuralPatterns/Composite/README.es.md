# Patrón Composite

El patrón Composite compone objetos en estructuras de árbol para representar jerarquías parte-todo. Permite a los clientes tratar objetos individuales y composiciones de objetos de manera uniforme.

## Estructura

- **Component**: Declara la interfaz para los objetos en la composición.
- **Leaf**: Representa los objetos hoja en la composición.
- **Composite**: Define el comportamiento para los componentes que tienen hijos.

## Ejemplo

```csharp
var leaf = new Leaf();
var composite = new Composite();
composite.Add(leaf);
composite.Operation(); // Output: Composite\nHoja
```

## Cuándo usarlo

- Cuando se quiere representar jerarquías parte-todo de objetos.
- Cuando los clientes deben poder tratar objetos individuales y composiciones de manera uniforme.
