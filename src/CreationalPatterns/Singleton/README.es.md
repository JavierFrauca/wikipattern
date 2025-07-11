# Patrón Singleton

El patrón Singleton asegura que una clase tenga solo una instancia y proporciona un punto de acceso global a ella.

## Estructura

- **Singleton**: Define un método estático para acceder a su instancia única.

## Ejemplo

```csharp
Singleton.Instance.Data = "Hola";
Console.WriteLine(Singleton.Instance.Data); // Output: Hola
```

## Cuándo usarlo

- Cuando se necesita exactamente una instancia de una clase.
- Cuando se requiere un punto de acceso global a la instancia.
