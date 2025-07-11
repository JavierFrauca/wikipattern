using System;
using System.Threading;
using System.Threading.Tasks;

// ============================================================================
// CIRCUIT BREAKER PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Protección de servicios externos (APIs, bases de datos)
// ============================================================================

public enum CircuitBreakerState
{
    Closed,    // Funcionamiento normal
    Open,      // Circuito abierto - rechaza llamadas
    HalfOpen   // Permitiendo llamadas de prueba
}

public class CircuitBreakerException : Exception
{
    public CircuitBreakerException(string message) : base(message) { }
}

// Circuit Breaker robusto para uso en producción
public class CircuitBreaker
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly TimeSpan _retryTimeout;
    private readonly object _lock = new object();
    
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private DateTime _nextAttemptTime = DateTime.MinValue;

    public CircuitBreakerState State => _state;
    public int FailureCount => _failureCount;
    public TimeSpan TimeSinceLastFailure => DateTime.UtcNow - _lastFailureTime;

    public CircuitBreaker(int failureThreshold = 5, 
                         TimeSpan? timeout = null, 
                         TimeSpan? retryTimeout = null)
    {
        _failureThreshold = failureThreshold;
        _timeout = timeout ?? TimeSpan.FromSeconds(10);
        _retryTimeout = retryTimeout ?? TimeSpan.FromSeconds(30);
    }

    // Ejecutar operación con protección del Circuit Breaker
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, string operationName = "Unknown")
    {
        CheckState(operationName);

        try
        {
            var timeoutCancellation = new CancellationTokenSource(_timeout);
            var result = await operation().ConfigureAwait(false);
            
            OnSuccess();
            LogOperation($"✅ {operationName} - Éxito", ConsoleColor.Green);
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex, operationName);
            throw;
        }
    }

    // Versión síncrona
    public T Execute<T>(Func<T> operation, string operationName = "Unknown")
    {
        CheckState(operationName);

        try
        {
            var result = operation();
            OnSuccess();
            LogOperation($"✅ {operationName} - Éxito", ConsoleColor.Green);
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex, operationName);
            throw;
        }
    }

    private void CheckState(string operationName)
    {
        lock (_lock)
        {
            switch (_state)
            {
                case CircuitBreakerState.Open:
                    if (DateTime.UtcNow >= _nextAttemptTime)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        LogOperation($"🔄 {operationName} - Circuit Breaker: HALF-OPEN (Intentando reconexión)", ConsoleColor.Yellow);
                    }
                    else
                    {
                        var waitTime = _nextAttemptTime - DateTime.UtcNow;
                        LogOperation($"🚫 {operationName} - Circuit Breaker: OPEN (Reintentar en {waitTime.TotalSeconds:F1}s)", ConsoleColor.Red);
                        throw new CircuitBreakerException(
                            $"Circuit breaker está ABIERTO. Reintentar en {waitTime.TotalSeconds:F1} segundos");
                    }
                    break;

                case CircuitBreakerState.HalfOpen:
                    LogOperation($"⚡ {operationName} - Circuit Breaker: HALF-OPEN (Prueba de reconexión)", ConsoleColor.Yellow);
                    break;

                case CircuitBreakerState.Closed:
                    LogOperation($"🟢 {operationName} - Circuit Breaker: CLOSED (Normal)", ConsoleColor.Green);
                    break;
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            _failureCount = 0;
            _state = CircuitBreakerState.Closed;
            if (_state == CircuitBreakerState.HalfOpen)
            {
                LogOperation("✅ Circuit Breaker: Reconexión exitosa - Estado: CLOSED", ConsoleColor.Green);
            }
        }
    }

    private void OnFailure(Exception ex, string operationName)
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            LogOperation($"❌ {operationName} - Fallo #{_failureCount}: {ex.Message}", ConsoleColor.Red);

            if (_failureCount >= _failureThreshold || _state == CircuitBreakerState.HalfOpen)
            {
                _state = CircuitBreakerState.Open;
                _nextAttemptTime = DateTime.UtcNow.Add(_retryTimeout);
                LogOperation($"🔴 Circuit Breaker: ABIERTO - Fallos: {_failureCount}/{_failureThreshold}", ConsoleColor.Red);
            }
        }
    }

    private static void LogOperation(string message, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        Console.ForegroundColor = originalColor;
    }

    // Métrica para monitoreo
    public CircuitBreakerMetrics GetMetrics()
    {
        lock (_lock)
        {
            return new CircuitBreakerMetrics
            {
                State = _state,
                FailureCount = _failureCount,
                FailureThreshold = _failureThreshold,
                TimeSinceLastFailure = TimeSinceLastFailure,
                TimeUntilNextAttempt = _state == CircuitBreakerState.Open 
                    ? _nextAttemptTime - DateTime.UtcNow 
                    : TimeSpan.Zero
            };
        }
    }
}

public class CircuitBreakerMetrics
{
    public CircuitBreakerState State { get; set; }
    public int FailureCount { get; set; }
    public int FailureThreshold { get; set; }
    public TimeSpan TimeSinceLastFailure { get; set; }
    public TimeSpan TimeUntilNextAttempt { get; set; }
}

// ============================================================================
// EJEMPLO REALISTA: SERVICIO DE PAGOS EXTERNOS
// ============================================================================
public class PaymentService
{
    private readonly CircuitBreaker _circuitBreaker;
    private readonly Random _random = new Random();

    public PaymentService()
    {
        _circuitBreaker = new CircuitBreaker(
            failureThreshold: 3,
            timeout: TimeSpan.FromSeconds(5),
            retryTimeout: TimeSpan.FromSeconds(10)
        );
    }

    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency = "USD")
    {
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            // Simular llamada a API externa de pagos
            await Task.Delay(1000); // Simular latencia de red
            
            // Simular fallos ocasionales (30% probabilidad)
            if (_random.NextDouble() < 0.3)
            {
                throw new InvalidOperationException($"Error del servicio de pagos: Timeout o servicio no disponible");
            }

            return new PaymentResult
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = amount,
                Currency = currency,
                Status = "Approved",
                Timestamp = DateTime.UtcNow
            };
        }, "ProcessPayment");
    }

    public CircuitBreakerMetrics GetHealthMetrics() => _circuitBreaker.GetMetrics();
}

public class PaymentResult
{
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }
    public DateTime Timestamp { get; set; }

    public override string ToString()
    {
        return $"💳 Transacción {TransactionId[..8]}... - {Amount:C} {Currency} - {Status}";
    }
}

// ============================================================================
// DEMO INTERACTIVO
// ============================================================================
public static class CircuitBreakerDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("💳 DEMO: CIRCUIT BREAKER CON SERVICIO DE PAGOS");
        Console.WriteLine("===============================================");

        var paymentService = new PaymentService();
        var payments = new[] { 100m, 250m, 75m, 500m, 150m, 300m, 80m, 420m, 180m, 650m };

        for (int i = 0; i < payments.Length; i++)
        {
            Console.WriteLine($"\n🔄 Procesando pago #{i + 1} de ${payments[i]}");
            
            try
            {
                var result = await paymentService.ProcessPaymentAsync(payments[i]);
                Console.WriteLine($"   ✅ {result}");
            }
            catch (CircuitBreakerException ex)
            {
                Console.WriteLine($"   🚫 Circuit Breaker: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Error de pago: {ex.Message}");
            }

            // Mostrar métricas del Circuit Breaker
            var metrics = paymentService.GetHealthMetrics();
            Console.WriteLine($"   📊 Estado: {metrics.State} | Fallos: {metrics.FailureCount}/{metrics.FailureThreshold}");

            // Pausa entre intentos
            await Task.Delay(2000);
        }

        Console.WriteLine("\n✅ Demo completado");
    }
}

// Para ejecutar el demo:
// await CircuitBreakerDemo.RunDemo();
