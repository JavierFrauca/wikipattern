# Retry Pattern

## 📋 Description

The **Retry Pattern** is a resilience pattern that allows automatic retrying of operations that may fail temporarily due to network issues, unavailable services, or transient conditions.

## 🎯 Purpose

- **Fault tolerance**: Handles transient errors automatically
- **Improved reliability**: Reduces failure probability from temporary issues
- **User experience**: Avoids unnecessary errors for end users
- **System resilience**: Makes applications more robust

## ✅ When to Use

- **External services**: API calls, databases, web services
- **Network operations**: Downloads, uploads, remote connections
- **Shared resources**: File access, message queues
- **Cloud services**: Operations that may have variable latency

## ❌ When NOT to Use

- **Logic errors**: Code bugs that will always fail
- **Authorization issues**: Incorrect credentials
- **Data validation**: Malformed or invalid data
- **Non-existent resources**: URLs that don't exist, deleted files

## 🏗️ Structure

```text
Client → RetryPolicy → TargetOperation
    ↓
[Attempt 1] → Failure
[Attempt 2] → Failure  
[Attempt 3] → Success ✓
```

## 💡 Implementation

The pattern is implemented by defining:

1. **Maximum number of attempts**
2. **Wait strategy** (linear, exponential, etc.)
3. **Retry conditions** (which errors to retry)
4. **Timeout per attempt**

## 📊 Usage Example

```csharp
// Basic configuration
var retryPolicy = new RetryPolicy(maxAttempts: 3);

// Operation that may fail
await retryPolicy.ExecuteAsync(async () =>
{
    var response = await httpClient.GetAsync("https://api.example.com/data");
    return await response.Content.ReadAsStringAsync();
});

// With advanced configuration
var advancedRetry = new RetryPolicy(
    maxAttempts: 5,
    baseDelay: TimeSpan.FromSeconds(1),
    backoffStrategy: BackoffStrategy.Exponential,
    maxDelay: TimeSpan.FromSeconds(30)
);
```

## 🔧 Pattern Variations

### 1. **Linear Backoff**

```csharp
// Constant wait between attempts
delay = baseDelay; // 1s, 1s, 1s, 1s...
```

### 2. **Exponential Backoff**

```csharp
// Exponential wait with jitter
delay = baseDelay * Math.Pow(2, attempt) + jitter;
// 1s, 2s, 4s, 8s...
```

### 3. **Circuit Breaker Integration**

```csharp
// Combined with Circuit Breaker
if (circuitBreaker.State == CircuitState.Open)
    throw new CircuitBreakerOpenException();
```

## ⚡ Performance Considerations

- **Total timeout**: Prevent application from hanging
- **Jitter**: Add randomness to avoid "thundering herd"
- **Resources**: Release connections between attempts
- **Logging**: Record attempts for diagnostics

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

## 🔗 Related Patterns

- **[Circuit Breaker](../CircuitBreaker/)**: Prevents attempts when service is down
- **[Timeout](../Timeout/)**: Defines time limit per operation
- **[Bulkhead](../Bulkhead/)**: Isolates resources to prevent cascading failures

## 📚 Additional Resources

- [Microsoft: Retry Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry)
- [AWS: Retry Logic](https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/)
- [Polly: .NET Resilience Library](https://github.com/App-vNext/Polly)

---

> 💡 **Tip**: Always combine Retry with timeout to prevent slow operations from consuming too many system resources.
