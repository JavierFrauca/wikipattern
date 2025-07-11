# Patrón Adapter

El patrón Adapter permite que objetos con interfaces incompatibles trabajen juntos envolviendo su propia interfaz alrededor de la de una clase existente.

## Estructura

- **Target**: Define la interfaz específica del dominio usada por el cliente.
- **Adapter**: Adapta la interfaz de Adaptee a la interfaz Target.
- **Adaptee**: Define una interfaz existente que necesita ser adaptada.

## Ejemplo

```csharp
var adaptee = new Adaptee();
ITarget target = new Adapter(adaptee);
target.Request(); // Output: Llamada específica del Adaptee
```

## Cuándo usarlo

- Cuando quieres usar una clase existente, pero su interfaz no coincide con la que necesitas.
- Para hacer que clases no relacionadas trabajen juntas.
