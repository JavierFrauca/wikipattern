# Patrón Repository

El patrón Repository media entre el dominio y la capa de acceso a datos, actuando como una colección en memoria de objetos de dominio. Desacopla la lógica de negocio de la lógica de acceso a datos.

## Estructura

- **Repository**: Interfaz para operaciones de acceso a datos.
- **InMemoryRepository**: Implementación de ejemplo.

## Ejemplo

```csharp
IRepository<string> repo = new InMemoryRepository<string>();
repo.Add("data"); // Output: Added: data
```

## Cuándo usarlo

- Cuando quieres desacoplar la lógica de negocio de la lógica de acceso a datos.
- Para proporcionar una interfaz simple para operaciones de datos.
