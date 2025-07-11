# Timeout Pattern - Patrón de Tiempo Límite

## 📋 Descripción

El **Patrón Timeout** establece un límite de tiempo máximo para completar una operación, evitando que operaciones lentas o colgadas bloqueen indefinidamente el sistema.

## 🎯 Propósito

- **Prevenir bloqueos**: Evita que operaciones se cuelguen indefinidamente
- **Recursos limitados**: Libera threads y conexiones en tiempo determinado
- **Experiencia de usuario**: Proporciona respuesta en tiempo razonable
- **SLA**: Garantiza tiempos de respuesta dentro de los acuerdos de servicio

## ✅ Cuándo Usarlo

- **Llamadas a servicios externos**: APIs, bases de datos, servicios web
- **Operaciones de red**: HTTP requests, socket connections
- **Operaciones de I/O**: Lectura de archivos, acceso a disco
- **Procesamiento pesado**: Algoritmos que pueden tardar mucho

## ❌ Cuándo NO Usarlo

- **Operaciones rápidas**: Cálculos simples que siempre son rápidos
- **Procesos críticos**: Donde la interrupción puede causar inconsistencias
- **Operaciones atómicas**: Transacciones que deben completarse o fallar completamente

## 🏗️ Estructura

```mermaid
graph LR
    A[Client] --> B[TimeoutWrapper]
    B --> C[Operation]
    B --> D[Timer]
    D -->|Timeout| E[Cancel Operation]
```

## 💡 Implementación Básica

```csharp
public class TimeoutWrapper
{
    public async Task<T> ExecuteWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        
        try
        {
            return await operation(cts.Token);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Operation timed out after {timeout}");
        }
    }
}
```

## 📊 Ejemplo de Uso

```csharp
public class HttpClientWithTimeout
{
    private readonly HttpClient _httpClient;
    private readonly TimeoutWrapper _timeoutWrapper;
    
    public HttpClientWithTimeout()
    {
        _httpClient = new HttpClient();
        _timeoutWrapper = new TimeoutWrapper();
    }
    
    public async Task<string> GetDataAsync(string url, TimeSpan timeout)
    {
        return await _timeoutWrapper.ExecuteWithTimeoutAsync(
            async (cancellationToken) =>
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            },
            timeout
        );
    }
}

// Uso
var client = new HttpClientWithTimeout();
try
{
    var data = await client.GetDataAsync(
        "https://api.example.com/data", 
        TimeSpan.FromSeconds(5)
    );
    Console.WriteLine($"Data received: {data}");
}
catch (TimeoutException ex)
{
    Console.WriteLine($"Request timed out: {ex.Message}");
}
```

## 🔧 Variaciones del Patrón

### 1. **Timeout Configurable**

```csharp
public class ConfigurableTimeoutService
{
    private readonly TimeSpan _defaultTimeout;
    private readonly Dictionary<string, TimeSpan> _operationTimeouts;
    
    public ConfigurableTimeoutService(IConfiguration config)
    {
        _defaultTimeout = TimeSpan.FromSeconds(
            config.GetValue<int>("DefaultTimeoutSeconds", 30));
        
        _operationTimeouts = config
            .GetSection("OperationTimeouts")
            .Get<Dictionary<string, TimeSpan>>() ?? new();
    }
    
    public async Task<T> ExecuteAsync<T>(
        string operationName,
        Func<CancellationToken, Task<T>> operation)
    {
        var timeout = _operationTimeouts.TryGetValue(operationName, out var customTimeout)
            ? customTimeout
            : _defaultTimeout;
            
        using var cts = new CancellationTokenSource(timeout);
        return await operation(cts.Token);
    }
}
```

### 2. **Timeout con Fallback**

```csharp
public class TimeoutWithFallback<T>
{
    private readonly Func<Task<T>> _fallbackOperation;
    
    public TimeoutWithFallback(Func<Task<T>> fallbackOperation)
    {
        _fallbackOperation = fallbackOperation;
    }
    
    public async Task<T> ExecuteAsync(
        Func<CancellationToken, Task<T>> primaryOperation,
        TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        
        try
        {
            return await primaryOperation(cts.Token);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
        {
            // Ejecutar fallback en caso de timeout
            return await _fallbackOperation();
        }
    }
}
```

### 3. **Timeout Progresivo**

```csharp
public class ProgressiveTimeout
{
    public async Task<T> ExecuteWithProgressiveTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan initialTimeout,
        int maxAttempts = 3,
        double timeoutMultiplier = 1.5)
    {
        var currentTimeout = initialTimeout;
        
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var cts = new CancellationTokenSource(currentTimeout);
            
            try
            {
                return await operation(cts.Token);
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                if (attempt == maxAttempts)
                    throw new TimeoutException($"All {maxAttempts} attempts timed out");
                
                // Incrementar timeout para siguiente intento
                currentTimeout = TimeSpan.FromMilliseconds(
                    currentTimeout.TotalMilliseconds * timeoutMultiplier);
            }
        }
        
        throw new InvalidOperationException("Unexpected end of retry loop");
    }
}
```

## 🎯 Ejemplo: Servicio de Base de Datos

```csharp
public class DatabaseService
{
    private readonly IDbConnection _connection;
    private readonly TimeoutWrapper _timeoutWrapper;
    
    public DatabaseService(IDbConnection connection)
    {
        _connection = connection;
        _timeoutWrapper = new TimeoutWrapper();
    }
    
    public async Task<List<Customer>> GetCustomersAsync(
        int pageSize = 50, 
        TimeSpan? timeout = null)
    {
        var operationTimeout = timeout ?? TimeSpan.FromSeconds(10);
        
        return await _timeoutWrapper.ExecuteWithTimeoutAsync(
            async (cancellationToken) =>
            {
                var command = _connection.CreateCommand();
                command.CommandText = "SELECT * FROM Customers LIMIT @pageSize";
                command.Parameters.Add(new SqlParameter("@pageSize", pageSize));
                
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                var customers = new List<Customer>();
                
                while (await reader.ReadAsync(cancellationToken))
                {
                    customers.Add(new Customer
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        Email = reader.GetString("Email")
                    });
                }
                
                return customers;
            },
            operationTimeout
        );
    }
}
```

## ⚡ Consideraciones de Rendimiento

- **Granularidad**: Definir timeouts apropiados para cada tipo de operación
- **Cleanup**: Asegurar liberación de recursos al cancelar
- **Monitoring**: Registrar timeouts para ajustar configuraciones
- **Cascading**: Evitar timeouts muy cortos que causen fallos en cascada

## 🧪 Testing

```csharp
[Test]
public async Task Timeout_ShouldCancelLongRunningOperation()
{
    // Arrange
    var timeoutWrapper = new TimeoutWrapper();
    var timeout = TimeSpan.FromMilliseconds(100);
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<TimeoutException>(() =>
        timeoutWrapper.ExecuteWithTimeoutAsync(
            async (ct) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1), ct);
                return "Should not complete";
            },
            timeout
        )
    );
    
    Assert.Contains("timed out after", exception.Message);
}

[Test]
public async Task Timeout_ShouldCompleteWithinTimeLimit()
{
    // Arrange
    var timeoutWrapper = new TimeoutWrapper();
    var timeout = TimeSpan.FromSeconds(1);
    
    // Act
    var result = await timeoutWrapper.ExecuteWithTimeoutAsync(
        async (ct) =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            return "Completed";
        },
        timeout
    );
    
    // Assert
    Assert.AreEqual("Completed", result);
}
```

## 📊 Métricas y Monitoreo

```csharp
public class TimeoutMetrics
{
    private readonly IMetrics _metrics;
    
    public async Task<T> ExecuteWithMetricsAsync<T>(
        string operationName,
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var cts = new CancellationTokenSource(timeout);
            var result = await operation(cts.Token);
            
            _metrics.Timer($"operation.duration.{operationName}")
                   .Record(stopwatch.Elapsed);
            
            return result;
        }
        catch (OperationCanceledException)
        {
            _metrics.Counter($"operation.timeout.{operationName}")
                   .Increment();
            throw;
        }
    }
}
```

## 🔗 Patrones Relacionados

- **[Retry](../Retry/)**: Retry puede usar timeout para cada intento
- **[Circuit Breaker](../CircuitBreaker/)**: Timeout ayuda a detectar servicios lentos
- **[Bulkhead](../Bulkhead/)**: Aísla recursos que pueden tener timeouts diferentes

## 📚 Recursos Adicionales

- [Microsoft: Timeout in HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.timeout)
- [Task Cancellation](https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)
- [CancellationToken Best Practices](https://devblogs.microsoft.com/premier-developer/recommended-patterns-for-cancellationtoken/)

---

> 💡 **Tip**: Siempre propaga CancellationToken en métodos async para permitir cancelación cooperativa y timeouts efectivos.
