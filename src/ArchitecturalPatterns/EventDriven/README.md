# Event Driven Programming/Architecture

## 📋 Description

**Event Driven Programming** is a programming paradigm where the program flow is determined by events such as user actions, messages from other programs or threads, or system state changes. In microservices architectures, this pattern enables creating highly decoupled and scalable systems.

## 🎯 Purpose

### Main Objectives

- **Decoupling**: Services don't depend directly on each other
- **Scalability**: Each service can scale independently
- **Resilience**: A failure in one service doesn't affect others
- **Extensibility**: Easy to add new reactive services
- **Auditability**: Complete trail of all system events

## 🏗️ Pattern Structure

### Key Components

1. **Event**: Representation of something that has occurred
2. **Event Bus**: Publication and subscription mechanism
3. **Event Handler**: Component that reacts to specific events
4. **Event Store**: Event persistence for auditing/replay
5. **Event Publisher**: Who publishes events to the bus

## 💻 C# Implementation

### Basic Structure

```csharp
// Base interface for events
public interface IEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
    string Source { get; }
}

// Handler to process events
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

// Event bus for pub/sub
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
}
```

### Domain Events

```csharp
public class OrderSubmittedEvent : BaseEvent
{
    public override string EventType => "OrderSubmitted";
    public override string Source => "OrderService";
    
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemData> Items { get; set; } = new();
}

public class OrderPaymentProcessedEvent : BaseEvent
{
    public override string EventType => "OrderPaymentProcessed";
    public override string Source => "PaymentService";
    
    public Guid OrderId { get; set; }
    public bool Success { get; set; }
    public string TransactionId { get; set; }
}
```

### Event Handlers

```csharp
public class OrderEventHandler : 
    IEventHandler<OrderSubmittedEvent>,
    IEventHandler<OrderPaymentProcessedEvent>
{
    private readonly IEventBus _eventBus;

    public async Task HandleAsync(OrderSubmittedEvent @event, CancellationToken cancellationToken = default)
    {
        // Validate order
        var isValid = ValidateOrder(@event);
        
        // Publish validation result
        await _eventBus.PublishAsync(new OrderValidatedEvent
        {
            OrderId = @event.OrderId,
            IsValid = isValid
        });
        
        if (isValid)
        {
            // Request payment processing
            await _eventBus.PublishAsync(new PaymentRequestedEvent
            {
                OrderId = @event.OrderId,
                Amount = @event.TotalAmount
            });
        }
    }
}
```

## 🔄 Pattern Flow

### Typical E-commerce Process

1. **Customer submits order** → `OrderSubmittedEvent`
2. **System validates order** → `OrderValidatedEvent`
3. **Reserve inventory** → `InventoryReservedEvent`
4. **Process payment** → `PaymentProcessedEvent`
5. **Complete order** → `OrderFulfilledEvent`
6. **Send notification** → `NotificationSentEvent`

### Flow Diagram

```text
[Customer] → [OrderService] → [EventBus] → [InventoryService]
                ↓                              ↓
           [EventStore]                [PaymentService]
                ↓                              ↓
        [AuditService] ← [EventBus] ← [NotificationService]
```

## ✅ Advantages

### 🚀 **Scalability**

- Each service scales independently
- Asynchronous processing improves performance
- Natural load distribution

### 🔧 **Maintainability**

- Highly decoupled services
- Easy to add new functionalities
- Localized changes in each service

### 🛡️ **Resilience**

- Tolerance to individual service failures
- Event replay capability
- Automatic recovery

### 📊 **Observability**

- Complete event traceability
- Natural system auditing
- Facilitated debugging

## ⚠️ Disadvantages

### 🔄 **Complexity**

- Distributed logic difficult to debug
- Eventual consistency
- More complex failure handling

### 📈 **Overhead**

- Additional latency from asynchronous communication
- More complex infrastructure
- Sophisticated monitoring needed

### 🎯 **Consistency**

- No distributed ACID transactions
- Possible inconsistent intermediate states
- Need for compensating actions

## 🎮 Ideal Use Cases

### ✅ **Recommended for**

- **E-commerce**: Complex order processing
- **Banking**: Transfers and transactions
- **IoT**: Real-time telemetry processing
- **Gaming**: Matchmaking and game event systems
- **Logistics**: Shipping and delivery tracking

### ❌ **Not recommended for**

- Simple applications with few components
- Systems requiring immediate consistency
- Critical synchronous operations
- Teams without distributed systems experience

## 🔧 Implementation Considerations

### Event Store

```csharp
public interface IEventStore
{
    Task SaveEventAsync(IEvent @event);
    Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId);
    Task<IEnumerable<IEvent>> GetEventsByTypeAsync(string eventType);
}
```

### Delivery Guarantees

- **At Least Once**: Event may be processed multiple times
- **At Most Once**: Event is processed at most once
- **Exactly Once**: Event is processed exactly once (more difficult)

### Idempotency

```csharp
public class IdempotentEventHandler : IEventHandler<OrderSubmittedEvent>
{
    private readonly HashSet<Guid> _processedEvents = new();

    public async Task HandleAsync(OrderSubmittedEvent @event, CancellationToken cancellationToken = default)
    {
        if (_processedEvents.Contains(@event.Id))
        {
            Console.WriteLine($"Event already processed: {@event.Id}");
            return; // Skip processing
        }

        // Process event
        await ProcessOrder(@event);
        
        _processedEvents.Add(@event.Id);
    }
}
```

## 📊 Metrics and Monitoring

### Important KPIs

- **Event Throughput**: Events processed per second
- **Processing Latency**: Average processing time
- **Failed Events**: Percentage of failed events
- **Queue Depth**: Event queue depth
- **Handler Response Time**: Response time per handler

### Monitoring Example

```csharp
public class EventMetrics
{
    public void RecordEventProcessed(string eventType, TimeSpan duration)
    {
        Console.WriteLine($"📊 Metric: {eventType} processed in {duration.TotalMilliseconds}ms");
        // Send to metrics system (Prometheus, etc.)
    }
    
    public void RecordEventFailed(string eventType, Exception exception)
    {
        Console.WriteLine($"❌ Error: {eventType} failed - {exception.Message}");
        // Send to alerting system
    }
}
```

## 🧪 Testing

### Unit Testing

```csharp
[Test]
public async Task OrderHandler_ShouldPublishValidationEvent_WhenOrderSubmitted()
{
    // Arrange
    var mockEventBus = new Mock<IEventBus>();
    var handler = new OrderEventHandler(mockEventBus.Object, null);
    var orderEvent = new OrderSubmittedEvent 
    { 
        OrderId = Guid.NewGuid(),
        TotalAmount = 100m 
    };

    // Act
    await handler.HandleAsync(orderEvent);

    // Assert
    mockEventBus.Verify(x => x.PublishAsync(
        It.Is<OrderValidatedEvent>(e => e.OrderId == orderEvent.OrderId)), 
        Times.Once);
}
```

### Integration Testing

```csharp
[Test]
public async Task EventFlow_ShouldProcessCompleteOrderWorkflow()
{
    // Arrange
    var eventBus = new InMemoryEventBus();
    var eventStore = new InMemoryEventStore();
    var completedOrders = new List<Guid>();
    
    // Setup handlers
    eventBus.Subscribe<OrderFulfilledEvent>(new TestOrderCompletionHandler(completedOrders));

    // Act
    await eventBus.PublishAsync(new OrderSubmittedEvent { OrderId = orderId });
    await Task.Delay(5000); // Wait for async processing

    // Assert
    Assert.Contains(orderId, completedOrders);
}
```

## 🔄 Relationship with Other Patterns

### **CQRS + Event Driven**

```csharp
// Commands trigger events
public class CreateOrderCommandHandler
{
    public async Task Handle(CreateOrderCommand command)
    {
        var order = new Order(command);
        await _repository.SaveAsync(order);
        
        await _eventBus.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });
    }
}
```

### **Event Sourcing + Event Driven**

```csharp
// Events store state AND trigger actions
public class OrderAggregate
{
    public void Apply(OrderCreatedEvent @event)
    {
        // Update internal state
        Id = @event.OrderId;
        CustomerId = @event.CustomerId;
        
        // Trigger side effects via event bus
        _eventBus.PublishAsync(new OrderProcessingRequestedEvent 
        { 
            OrderId = @event.OrderId 
        });
    }
}
```

### **Saga Pattern + Event Driven**

```csharp
public class OrderProcessingSaga : 
    IEventHandler<OrderSubmittedEvent>,
    IEventHandler<PaymentProcessedEvent>,
    IEventHandler<InventoryReservedEvent>
{
    // Orchestrates long-running business processes
    // using event-driven coordination
}
```

## 🚀 Practical Example

The code includes a complete example of an e-commerce system that demonstrates:

1. **Order processing**: From submission to delivery
2. **Multiple services**: Order, Inventory, Payment, Notification
3. **Failure handling**: Automatic compensation
4. **Auditing**: Complete event tracking
5. **Scalability**: Asynchronous processing
6. **Monitoring**: Real-time metrics

### Running the Demo

```bash
dotnet run
```

## 📚 Additional Resources

### Recommended Libraries

- **MassTransit**: Service bus for .NET
- **NServiceBus**: Messaging framework
- **EventStore**: Event-specific database
- **Apache Kafka**: Distributed streaming platform
- **RabbitMQ**: Message broker
- **Azure Service Bus**: Cloud messaging service

### Related Patterns

- **Event Sourcing**: Persist events as source of truth
- **CQRS**: Separate reads and writes
- **Saga Pattern**: Coordinate distributed transactions
- **Outbox Pattern**: Guarantee event delivery
- **Materialized Views**: Optimized views from events

---

## 📈 Conclusion

Event Driven Programming is fundamental for modern microservices architectures. It enables creating highly scalable and resilient systems, though it requires greater complexity in design and operation. It's ideal for complex domains where decoupling and scalability are priorities.

**Key consideration**: Implement gradually, starting with simple use cases and evolving toward more complex workflows as the team gains experience.
