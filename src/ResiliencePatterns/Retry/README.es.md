# Retry Pattern - Patrón de Reintento

## 📋 Descripción

El **Patrón Retry** es un patrón de resiliencia que permite reintentar automáticamente operaciones que pueden fallar temporalmente debido a problemas de red, servicios no disponibles, o condiciones transitorias.

## 🎯 Propósito

- **Tolerancia a fallos**: Maneja errores transitorios de manera automática
- **Mejora la confiabilidad**: Reduce la probabilidad de fallo por problemas temporales
- **Experiencia del usuario**: Evita errores innecesarios para el usuario final
- **Resilencia del sistema**: Permite que las aplicaciones sean más robustas

## ✅ Cuándo Usarlo

- **Servicios externos**: Llamadas a APIs, bases de datos, servicios web
- **Operaciones de red**: Descargas, uploads, conexiones remotas
- **Recursos compartidos**: Acceso a archivos, colas de mensajes
- **Servicios en la nube**: Operaciones que pueden tener latencia variable

## ❌ Cuándo NO Usarlo

- **Errores de lógica**: Bugs en el código que siempre fallarán
- **Problemas de autorización**: Credenciales incorrectas
- **Validación de datos**: Datos malformados o inválidos
- **Recursos inexistentes**: URLs que no existen, archivos eliminados

## 🏗️ Estructura

```
Cliente → RetryPolicy → OperaciónDestino
    ↓
[Intento 1] → Fallo
[Intento 2] → Fallo  
[Intento 3] → Éxito ✓
```

## 💡 Implementación

El patrón se implementa definiendo:

1. **Número máximo de intentos**
2. **Estrategia de espera** (linear, exponencial, etc.)
3. **Condiciones de reintento** (qué errores reintentar)
4. **Timeout por intento**

## 📊 Ejemplo de Uso

```csharp
// Configuración básica
var retryPolicy = new RetryPolicy(maxAttempts: 3);

// Operación que puede fallar
await retryPolicy.ExecuteAsync(async () =>
{
    var response = await httpClient.GetAsync("https://api.example.com/data");
    return await response.Content.ReadAsStringAsync();
});

// Con configuración avanzada
var advancedRetry = new RetryPolicy(
    maxAttempts: 5,
    baseDelay: TimeSpan.FromSeconds(1),
    backoffStrategy: BackoffStrategy.Exponential,
    maxDelay: TimeSpan.FromSeconds(30)
);
```

## 🔧 Variaciones del Patrón

### 1. **Linear Backoff**
```csharp
// Espera constante entre intentos
delay = baseDelay; // 1s, 1s, 1s, 1s...
```

### 2. **Exponential Backoff**
```csharp
// Espera exponencial con jitter
delay = baseDelay * Math.Pow(2, attempt) + jitter;
// 1s, 2s, 4s, 8s...
```

### 3. **Circuit Breaker Integration**
```csharp
// Combinado con Circuit Breaker
if (circuitBreaker.State == CircuitState.Open)
    throw new CircuitBreakerOpenException();
```

## ⚡ Consideraciones de Rendimiento

- **Timeout total**: Evitar que la aplicación se cuelgue
- **Jitter**: Añadir aleatoriedad para evitar "thundering herd"
- **Recursos**: Liberar conexiones entre intentos
- **Logging**: Registrar intentos para diagnóstico

## 🧪 Testing

```csharp
[Test]
public async Task Retry_ShouldSucceedAfterTransientFailure()
{
    // Arrange
    var mockService = new Mock<IExternalService>();
    mockService.SetupSequence(x => x.GetDataAsync())
        .ThrowsAsync(new TimeoutException())
        .ThrowsAsync(new HttpRequestException())
        .ReturnsAsync("Success");

    var retryPolicy = new RetryPolicy(maxAttempts: 3);

    // Act
    var result = await retryPolicy.ExecuteAsync(() => 
        mockService.Object.GetDataAsync());

    // Assert
    Assert.AreEqual("Success", result);
    mockService.Verify(x => x.GetDataAsync(), Times.Exactly(3));
}
```

## 🔗 Patrones Relacionados

- **[Circuit Breaker](../CircuitBreaker/)**: Previene intentos cuando el servicio está caído
- **[Timeout](../Timeout/)**: Define límite de tiempo por operación
- **[Bulkhead](../Bulkhead/)**: Aísla recursos para evitar fallos en cascada

## 📚 Recursos Adicionales

- [Microsoft: Retry Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry)
- [AWS: Retry Logic](https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/)
- [Polly: .NET Resilience Library](https://github.com/App-vNext/Polly)

---

> 💡 **Tip**: Siempre combina Retry con timeout para evitar que una operación lenta consuma demasiados recursos del sistema.
