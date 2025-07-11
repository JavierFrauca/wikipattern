# Patrón Test Double

El patrón Test Double utiliza objetos sustitutos (dobles) en lugar de dependencias reales durante las pruebas. Los tipos incluyen fakes, stubs, mocks y spies.

## Estructura

- **IService**: Interfaz de la dependencia.
- **RealService**: Implementación real.
- **FakeService**: Implementación doble de prueba.

## Ejemplo

```csharp
IService service = new FakeService();
Console.WriteLine(service.GetValue()); // Output: 1
```

## Cuándo usarlo

- Cuando necesitas aislar el código bajo prueba de dependencias externas.
- Para simular diferentes escenarios y comportamientos en las pruebas.
