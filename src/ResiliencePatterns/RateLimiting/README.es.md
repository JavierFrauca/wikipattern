# Rate Limiting Pattern - Patrón de Limitación de Velocidad

## 📋 Descripción

El **Patrón Rate Limiting** controla la velocidad a la que se permiten operaciones o solicitudes, protegiendo recursos contra sobrecarga y garantizando un uso justo entre múltiples clientes.

## 🎯 Propósito

- **Protección de recursos**: Evita sobrecarga de servicios y bases de datos
- **Calidad de servicio**: Garantiza rendimiento consistente para todos los usuarios
- **Prevención de abuso**: Evita spam y ataques de denegación de servicio
- **Control de costos**: Limita uso de recursos que pueden tener costo asociado

## ✅ Cuándo Usarlo

- **APIs públicas**: Limitar requests por usuario/IP
- **Servicios web**: Proteger contra tráfico excesivo
- **Bases de datos**: Controlar acceso a recursos costosos
- **Microservicios**: Proteger servicios downstream
- **Rate billing**: Servicios con facturación por uso

## ❌ Cuándo NO Usarlo

- **Servicios internos confiables**: Entre servicios del mismo sistema
- **Operaciones críticas**: Donde el delay no es aceptable
- **Recursos ilimitados**: Cuando no hay restricciones de capacidad
- **Procesos batch**: Operaciones que necesitan procesar grandes volúmenes

## 🏗️ Algoritmos de Rate Limiting

### 1. **Token Bucket**
- Tokens se añaden a velocidad constante
- Cada request consume un token
- Permite ráfagas cortas

### 2. **Fixed Window**
- Límite fijo por ventana de tiempo
- Simple pero puede tener efectos de borde

### 3. **Sliding Window**
- Ventana deslizante más suave
- Distribuye carga más uniformemente

### 4. **Leaky Bucket**
- Procesa requests a velocidad constante
- Suaviza picos de tráfico

## 💡 Implementación: Token Bucket

```csharp
public class TokenBucket
{
    private readonly int _capacity;
    private readonly double _refillRate;
    private readonly object _lock = new();
    
    private double _tokens;
    private DateTime _lastRefill;
    
    public TokenBucket(int capacity, double refillRate)
    {
        _capacity = capacity;
        _refillRate = refillRate; // tokens per second
        _tokens = capacity;
        _lastRefill = DateTime.UtcNow;
    }
    
    public bool TryConsume(int tokensRequired = 1)
    {
        lock (_lock)
        {
            RefillTokens();
            
            if (_tokens >= tokensRequired)
            {
                _tokens -= tokensRequired;
                return true;
            }
            
            return false;
        }
    }
    
    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var timePassed = (now - _lastRefill).TotalSeconds;
        var tokensToAdd = timePassed * _refillRate;
        
        _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
        _lastRefill = now;
    }
}
```

## 📊 Ejemplo de Uso: API Rate Limiting

```csharp
public class ApiRateLimiter
{
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();
    private readonly int _requestsPerMinute;
    private readonly int _burstCapacity;
    
    public ApiRateLimiter(int requestsPerMinute = 60, int burstCapacity = 10)
    {
        _requestsPerMinute = requestsPerMinute;
        _burstCapacity = burstCapacity;
    }
    
    public async Task<IActionResult> CheckRateLimit(
        string clientId, 
        Func<Task<IActionResult>> operation)
    {
        var bucket = _buckets.GetOrAdd(clientId, _ => 
            new TokenBucket(_burstCapacity, _requestsPerMinute / 60.0));
        
        if (!bucket.TryConsume())
        {
            return new StatusCodeResult(429); // Too Many Requests
        }
        
        return await operation();
    }
}

// Uso en controlador
[ApiController]
public class DataController : ControllerBase
{
    private readonly ApiRateLimiter _rateLimiter;
    
    public DataController(ApiRateLimiter rateLimiter)
    {
        _rateLimiter = rateLimiter;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var clientId = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return await _rateLimiter.CheckRateLimit(clientId, async () =>
        {
            var data = await GetDataFromService();
            return Ok(data);
        });
    }
}
```

## 🔧 Variaciones del Patrón

### 1. **Fixed Window Counter**

```csharp
public class FixedWindowRateLimiter
{
    private readonly ConcurrentDictionary<string, WindowCounter> _windows = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;
    
    public FixedWindowRateLimiter(int maxRequests, TimeSpan windowSize)
    {
        _maxRequests = maxRequests;
        _windowSize = windowSize;
    }
    
    public bool IsAllowed(string key)
    {
        var now = DateTime.UtcNow;
        var windowStart = new DateTime(
            now.Ticks / _windowSize.Ticks * _windowSize.Ticks);
        
        var counter = _windows.AddOrUpdate(key,
            _ => new WindowCounter(windowStart, 1),
            (_, existing) =>
            {
                if (existing.WindowStart == windowStart)
                {
                    return new WindowCounter(windowStart, existing.Count + 1);
                }
                return new WindowCounter(windowStart, 1);
            });
        
        return counter.Count <= _maxRequests;
    }
    
    private record WindowCounter(DateTime WindowStart, int Count);
}
```

### 2. **Sliding Window Log**

```csharp
public class SlidingWindowRateLimiter
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requestLogs = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;
    private readonly object _cleanupLock = new();
    
    public bool IsAllowed(string key)
    {
        var now = DateTime.UtcNow;
        var cutoff = now - _windowSize;
        
        var requests = _requestLogs.GetOrAdd(key, _ => new List<DateTime>());
        
        lock (requests)
        {
            // Limpiar requests antiguos
            requests.RemoveAll(time => time < cutoff);
            
            if (requests.Count >= _maxRequests)
                return false;
            
            requests.Add(now);
            return true;
        }
    }
}
```

### 3. **Distributed Rate Limiting (Redis)**

```csharp
public class DistributedRateLimiter
{
    private readonly IDatabase _redis;
    private readonly int _maxRequests;
    private readonly TimeSpan _window;
    
    public DistributedRateLimiter(IDatabase redis, int maxRequests, TimeSpan window)
    {
        _redis = redis;
        _maxRequests = maxRequests;
        _window = window;
    }
    
    public async Task<bool> IsAllowedAsync(string key)
    {
        var script = @"
            local current = redis.call('incr', KEYS[1])
            if current == 1 then
                redis.call('expire', KEYS[1], ARGV[1])
            end
            return current
        ";
        
        var count = await _redis.ScriptEvaluateAsync(
            script, 
            new RedisKey[] { $"rate_limit:{key}" },
            new RedisValue[] { (int)_window.TotalSeconds });
        
        return (int)count <= _maxRequests;
    }
}
```

## 🎯 Ejemplo: Servicio de Email

```csharp
public class EmailService
{
    private readonly TokenBucket _rateLimiter;
    private readonly IEmailProvider _emailProvider;
    
    public EmailService(IEmailProvider emailProvider)
    {
        _emailProvider = emailProvider;
        // 10 emails per minute, burst of 5
        _rateLimiter = new TokenBucket(5, 10.0 / 60.0);
    }
    
    public async Task<SendResult> SendEmailAsync(EmailMessage message)
    {
        if (!_rateLimiter.TryConsume())
        {
            return new SendResult
            {
                Success = false,
                Error = "Rate limit exceeded. Please try again later."
            };
        }
        
        try
        {
            await _emailProvider.SendAsync(message);
            return new SendResult { Success = true };
        }
        catch (Exception ex)
        {
            return new SendResult 
            { 
                Success = false, 
                Error = ex.Message 
            };
        }
    }
}
```

## ⚡ Consideraciones de Rendimiento

- **Memoria**: Los contadores consumen memoria, implementar limpieza periódica
- **Concurrencia**: Usar estructuras thread-safe para aplicaciones multi-thread
- **Persistencia**: Para rate limiting distribuido, usar Redis o similar
- **Granularidad**: Balancear entre precisión y rendimiento

## 🧪 Testing

```csharp
[Test]
public void TokenBucket_ShouldAllowRequestsWithinRate()
{
    // Arrange
    var bucket = new TokenBucket(capacity: 5, refillRate: 1.0); // 1 token per second
    
    // Act & Assert
    for (int i = 0; i < 5; i++)
    {
        Assert.IsTrue(bucket.TryConsume(), $"Request {i + 1} should be allowed");
    }
    
    // Should be exhausted
    Assert.IsFalse(bucket.TryConsume(), "Should be rate limited");
}

[Test]
public async Task TokenBucket_ShouldRefillOverTime()
{
    // Arrange
    var bucket = new TokenBucket(capacity: 2, refillRate: 2.0); // 2 tokens per second
    
    // Consume all tokens
    Assert.IsTrue(bucket.TryConsume());
    Assert.IsTrue(bucket.TryConsume());
    Assert.IsFalse(bucket.TryConsume());
    
    // Wait for refill
    await Task.Delay(TimeSpan.FromSeconds(1.1));
    
    // Should have refilled
    Assert.IsTrue(bucket.TryConsume());
    Assert.IsTrue(bucket.TryConsume());
}
```

## 📊 Métricas y Monitoreo

```csharp
public class RateLimitingMetrics
{
    private readonly IMetrics _metrics;
    
    public void RecordRequest(string clientId, bool allowed)
    {
        _metrics.Counter("rate_limit.requests")
               .WithTag("client", clientId)
               .WithTag("allowed", allowed.ToString())
               .Increment();
    }
    
    public void RecordTokenBucketState(string bucketId, double tokens)
    {
        _metrics.Gauge("rate_limit.tokens")
               .WithTag("bucket", bucketId)
               .Set(tokens);
    }
}
```

## 🔗 Patrones Relacionados

- **[Circuit Breaker](../CircuitBreaker/)**: Rate limiting puede prevenir condiciones que abren el circuit
- **[Bulkhead](../Bulkhead/)**: Ambos controlan acceso a recursos
- **[Timeout](../Timeout/)**: Complementa rate limiting con límites de tiempo

## 📚 Recursos Adicionales

- [Token Bucket Algorithm](https://en.wikipedia.org/wiki/Token_bucket)
- [Rate Limiting Strategies](https://cloud.google.com/solutions/rate-limiting-strategies-techniques)
- [ASP.NET Core Rate Limiting](https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/)
- [Redis Rate Limiting](https://redis.io/commands/incr#pattern-rate-limiter)

---

> 💡 **Tip**: Combina diferentes algoritmos según el caso de uso: Token Bucket para permitir ráfagas, Fixed Window para simplicidad, Sliding Window para distribución uniforme.
