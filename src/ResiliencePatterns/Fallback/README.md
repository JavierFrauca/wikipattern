# Fallback Pattern

## 📋 Description

The **Fallback Pattern** provides an alternative mechanism when the primary operation fails, ensuring that the system can continue functioning with reduced functionality rather than failing completely.

## 🎯 Purpose

- **Graceful degradation**: Maintain basic functionality when services fail
- **Service continuity**: Avoid complete system interruptions
- **User experience**: Provide useful response even if limited
- **Resilience**: Increase system fault tolerance

## ✅ When to Use

- **External services**: When APIs or services may be unavailable
- **Cached data**: Serve stale data when primary source fails
- **Multiple providers**: Automatically switch to secondary provider
- **Optional functionality**: Disable non-critical features

## ❌ When NOT to Use

- **Critical operations**: Where fallback could cause inconsistencies
- **Sensitive data**: When fallback cannot guarantee same security
- **Transactions**: Operations that must be atomic
- **Real-time**: When fallback latency is unacceptable

## 🏗️ Structure

```mermaid
graph TD
    A[Client] --> B[Primary Service]
    B -->|Success| C[Return Result]
    B -->|Failure| D[Fallback Service]
    D --> E[Return Fallback Result]
```

## 💡 Basic Implementation

```csharp
public class FallbackService<T>
{
    private readonly Func<Task<T>> _primaryOperation;
    private readonly Func<Task<T>> _fallbackOperation;
    
    public FallbackService(
        Func<Task<T>> primaryOperation,
        Func<Task<T>> fallbackOperation)
    {
        _primaryOperation = primaryOperation;
        _fallbackOperation = fallbackOperation;
    }
    
    public async Task<T> ExecuteAsync()
    {
        try
        {
            return await _primaryOperation();
        }
        catch (Exception ex)
        {
            // Log the primary failure
            Console.WriteLine($"Primary operation failed: {ex.Message}");
            
            try
            {
                return await _fallbackOperation();
            }
            catch (Exception fallbackEx)
            {
                // Log fallback failure and rethrow original exception
                Console.WriteLine($"Fallback also failed: {fallbackEx.Message}");
                throw new AggregateException(ex, fallbackEx);
            }
        }
    }
}
```

## 📊 Usage Example: Configuration Service

```csharp
public class ConfigurationService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _configUrl;
    
    public ConfigurationService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
        _configUrl = "https://config-service.company.com/api/config";
    }
    
    public async Task<AppConfig> GetConfigurationAsync()
    {
        var fallbackService = new FallbackService<AppConfig>(
            primaryOperation: GetConfigFromRemoteService,
            fallbackOperation: GetConfigFromCache
        );
        
        return await fallbackService.ExecuteAsync();
    }
    
    private async Task<AppConfig> GetConfigFromRemoteService()
    {
        var response = await _httpClient.GetAsync(_configUrl);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var config = JsonSerializer.Deserialize<AppConfig>(json);
        
        // Cache successful response
        _cache.Set("app_config", config, TimeSpan.FromMinutes(10));
        
        return config;
    }
    
    private async Task<AppConfig> GetConfigFromCache()
    {
        if (_cache.TryGetValue("app_config", out AppConfig cachedConfig))
        {
            return cachedConfig;
        }
        
        // Last resort: default configuration
        return new AppConfig
        {
            DatabaseConnectionString = "Data Source=localhost;Initial Catalog=DefaultDB",
            FeatureFlags = new Dictionary<string, bool>
            {
                ["NewFeature"] = false,
                ["AdvancedMode"] = false
            },
            MaxRetries = 3,
            TimeoutSeconds = 30
        };
    }
}
```

## 🔧 Pattern Variations

### 1. **Chained Fallback**

```csharp
public class ChainedFallbackService<T>
{
    private readonly List<Func<Task<T>>> _operations;
    
    public ChainedFallbackService(params Func<Task<T>>[] operations)
    {
        _operations = new List<Func<Task<T>>>(operations);
    }
    
    public async Task<T> ExecuteAsync()
    {
        var exceptions = new List<Exception>();
        
        foreach (var operation in _operations)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        
        throw new AggregateException(
            "All fallback operations failed", exceptions);
    }
}

// Usage
var weatherService = new ChainedFallbackService<WeatherData>(
    () => _primaryWeatherApi.GetWeatherAsync(),
    () => _secondaryWeatherApi.GetWeatherAsync(),
    () => _cachedWeatherService.GetLastKnownWeatherAsync(),
    () => Task.FromResult(GetDefaultWeatherData())
);
```

### 2. **Fallback with Circuit Breaker**

```csharp
public class FallbackWithCircuitBreaker<T>
{
    private readonly CircuitBreaker _circuitBreaker;
    private readonly Func<Task<T>> _primaryOperation;
    private readonly Func<Task<T>> _fallbackOperation;
    
    public async Task<T> ExecuteAsync()
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(_primaryOperation);
        }
        catch (CircuitBreakerOpenException)
        {
            // Circuit is open, go directly to fallback
            return await _fallbackOperation();
        }
    }
}
```

### 3. **Conditional Fallback**

```csharp
public class ConditionalFallbackService<T>
{
    private readonly Func<Task<T>> _primaryOperation;
    private readonly Dictionary<Type, Func<Task<T>>> _fallbackStrategies;
    
    public ConditionalFallbackService(Func<Task<T>> primaryOperation)
    {
        _primaryOperation = primaryOperation;
        _fallbackStrategies = new Dictionary<Type, Func<Task<T>>>();
    }
    
    public ConditionalFallbackService<T> OnException<TException>(Func<Task<T>> fallback)
        where TException : Exception
    {
        _fallbackStrategies[typeof(TException)] = fallback;
        return this;
    }
    
    public async Task<T> ExecuteAsync()
    {
        try
        {
            return await _primaryOperation();
        }
        catch (Exception ex)
        {
            var exceptionType = ex.GetType();
            
            // Look for specific fallback for this exception type
            if (_fallbackStrategies.TryGetValue(exceptionType, out var fallback))
            {
                return await fallback();
            }
            
            // Look for fallback for base types
            foreach (var (type, strategy) in _fallbackStrategies)
            {
                if (type.IsAssignableFrom(exceptionType))
                {
                    return await strategy();
                }
            }
            
            throw; // No fallback for this exception
        }
    }
}
```

## 🎯 Example: Payment Service

```csharp
public class PaymentService
{
    private readonly IPaymentProvider _primaryProvider;
    private readonly IPaymentProvider _fallbackProvider;
    private readonly IPaymentLogger _logger;
    
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var fallbackService = new FallbackService<PaymentResult>(
            primaryOperation: () => ProcessWithPrimary(request),
            fallbackOperation: () => ProcessWithFallback(request)
        );
        
        var result = await fallbackService.ExecuteAsync();
        
        // Log if fallback was used
        if (result.UsedFallback)
        {
            await _logger.LogFallbackUsedAsync(request.TransactionId, result.Provider);
        }
        
        return result;
    }
    
    private async Task<PaymentResult> ProcessWithPrimary(PaymentRequest request)
    {
        var result = await _primaryProvider.ProcessAsync(request);
        return new PaymentResult
        {
            Success = result.Success,
            TransactionId = result.TransactionId,
            Provider = "Primary",
            UsedFallback = false
        };
    }
    
    private async Task<PaymentResult> ProcessWithFallback(PaymentRequest request)
    {
        var result = await _fallbackProvider.ProcessAsync(request);
        return new PaymentResult
        {
            Success = result.Success,
            TransactionId = result.TransactionId,
            Provider = "Fallback",
            UsedFallback = true
        };
    }
}
```

## ⚡ Performance Considerations

- **Latency**: Fallback may add latency if primary is slow
- **Resources**: Maintaining connections to multiple services consumes resources
- **Consistency**: Ensure fallback provides consistent data
- **Monitoring**: Track fallback usage to identify issues

## 🧪 Testing

```csharp
[Test]
public async Task Fallback_ShouldUsePrimaryWhenAvailable()
{
    // Arrange
    var primaryCalled = false;
    var fallbackCalled = false;
    
    var fallbackService = new FallbackService<string>(
        primaryOperation: async () =>
        {
            primaryCalled = true;
            return "Primary Result";
        },
        fallbackOperation: async () =>
        {
            fallbackCalled = true;
            return "Fallback Result";
        }
    );
    
    // Act
    var result = await fallbackService.ExecuteAsync();
    
    // Assert
    Assert.AreEqual("Primary Result", result);
    Assert.IsTrue(primaryCalled);
    Assert.IsFalse(fallbackCalled);
}

[Test]
public async Task Fallback_ShouldUseFallbackWhenPrimaryFails()
{
    // Arrange
    var fallbackService = new FallbackService<string>(
        primaryOperation: () => throw new HttpRequestException("Service unavailable"),
        fallbackOperation: async () => "Fallback Result"
    );
    
    // Act
    var result = await fallbackService.ExecuteAsync();
    
    // Assert
    Assert.AreEqual("Fallback Result", result);
}
```

## 📊 Metrics and Monitoring

```csharp
public class FallbackMetrics
{
    private readonly IMetrics _metrics;
    
    public void RecordPrimarySuccess(string serviceName)
    {
        _metrics.Counter("fallback.primary_success")
               .WithTag("service", serviceName)
               .Increment();
    }
    
    public void RecordFallbackUsed(string serviceName, string reason)
    {
        _metrics.Counter("fallback.used")
               .WithTag("service", serviceName)
               .WithTag("reason", reason)
               .Increment();
    }
    
    public void RecordFallbackLatency(string serviceName, TimeSpan duration)
    {
        _metrics.Timer("fallback.latency")
               .WithTag("service", serviceName)
               .Record(duration);
    }
}
```

## 🔗 Related Patterns

- **[Circuit Breaker](../CircuitBreaker/)**: Can trigger fallbacks when open
- **[Retry](../Retry/)**: Executes before fallback
- **[Timeout](../Timeout/)**: Determines when to trigger fallback
- **[Cache-Aside](../../ArchitecturalPatterns/Cache/)**: Specific type of fallback

## 📚 Additional Resources

- [Microsoft: Fallback Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/fallback)
- [AWS: Fallback Strategies](https://aws.amazon.com/builders-library/avoiding-fallback-in-distributed-systems/)
- [Polly: .NET Resilience Library](https://github.com/App-vNext/Polly)

---

> 💡 **Tip**: Design fallbacks that provide degraded but useful functionality, not just error messages. Users prefer limited functionality over no functionality at all.
