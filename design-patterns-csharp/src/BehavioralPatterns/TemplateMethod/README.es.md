# Patrón Template Method

El patrón Template Method define el esqueleto de un algoritmo en un método, delegando algunos pasos a las subclases. Permite que las subclases redefinan ciertos pasos del algoritmo sin cambiar su estructura.

## Estructura

- **AbstractClass**: Define operaciones primitivas abstractas que deben ser implementadas por las subclases y un método plantilla que define la estructura del algoritmo.
- **ConcreteClass**: Implementa las operaciones primitivas.

## Ejemplo

```csharp
var obj = new ConcreteClass();
obj.TemplateMethod(); // Output: Paso 1\nPaso 2
```

## Cuándo usarlo

- Para implementar las partes invariantes de un algoritmo y dejar que las subclases implementen el comportamiento variable.
- Para controlar los puntos de extensión de un algoritmo.
