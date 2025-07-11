# Patrón Pipeline

El patrón Pipeline organiza el procesamiento en una secuencia de etapas, donde la salida de una etapa es la entrada de la siguiente. Cada etapa suele implementarse como un componente separado.

## Estructura

- **Step**: Representa una etapa en el pipeline.
- **Pipeline**: Gestiona la secuencia y ejecución de las etapas.

## Ejemplo

```csharp
var pipeline = new Pipeline(new StepA(), new StepB());
await pipeline.RunAsync(); // Output: Step A ejecutado\nStep B ejecutado
```

## Cuándo usarlo

- Cuando se quiere procesar datos en una serie de pasos.
- Para mejorar la modularidad y reutilización de la lógica de procesamiento.
