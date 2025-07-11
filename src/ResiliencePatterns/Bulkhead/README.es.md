# Patrón Bulkhead

El patrón Bulkhead aísla elementos de una aplicación en compartimentos para que, si uno falla, los demás sigan funcionando.

## Estructura

- **Bulkhead**: Controla el acceso concurrente a un recurso o sección.

## Ejemplo

```csharp
var bulkhead = new Bulkhead(2);
if (bulkhead.TryEnter()) { /* hacer trabajo */ bulkhead.Leave(); }
```

## Cuándo usarlo

- Para evitar que un fallo en una parte del sistema afecte a las demás.
- Para mejorar la resiliencia y el aislamiento de fallos.
