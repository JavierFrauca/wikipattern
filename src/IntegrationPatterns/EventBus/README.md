# Event Bus Pattern - Microservices Communication

## 📋 Overview

The **Event Bus** pattern for microservices provides a decoupled communication mechanism between distributed services through asynchronous event publishing and subscription. This implementation offers a robust, scalable solution for inter-service communication with features like retry logic, dead letter queues, and comprehensive monitoring.

## 🎯 Purpose

- **Decouple microservices** through asynchronous event-driven communication
- **Enable scalable integration** between distributed services
- **Provide fault tolerance** with retry mechanisms and error handling
- **Support event correlation** across service boundaries
- **Offer observability** through metrics and monitoring

## 🏗️ Implementation

### Core Components

1. **IIntegrationEvent**: Base interface for events traveling between microservices
2. **IntegrationEvent**: Abstract base class with correlation and metadata
3. **IIntegrationEventHandler**: Handler interface for processing events
4. **IMicroservicesEventBus**: Main interface for publishing and subscribing
5. **MicroservicesEventBus**: Complete implementation with advanced features

### Key Features

- **Asynchronous Processing**: Non-blocking event publishing and handling
- **Concurrent Workers**: Configurable number of worker threads
- **Retry Logic**: Exponential backoff for failed events
- **Dead Letter Queue**: Storage for permanently failed events
- **Correlation Tracking**: Event correlation across service boundaries
- **Metrics Collection**: Performance and reliability statistics
- **Cross-Service Subscription**: Services can subscribe to events from other services

## 💻 Usage Example

```csharp
// Configure Event Bus for each microservice
var orderService = new MicroservicesEventBus(new EventBusConfiguration 
{ 
    ServiceName = "OrderService",
    MaxConcurrentHandlers = 5
});

await orderService.StartAsync();

// Subscribe to events from other services
orderService.Subscribe("PaymentService", new PaymentCompletedHandler());

// Publish events to other services
await orderService.PublishAsync(new OrderCreatedEvent
{
    OrderId = Guid.NewGuid(),
    CustomerId = customerId,
    TotalAmount = 299.99m,
    CustomerEmail = "customer@example.com"
});
```

## 🔄 Event Flow Example

```
OrderService        PaymentService      ShippingService     NotificationService
     |                     |                    |                    |
     |-- OrderCreated ---->|                    |                    |
     |                     |                    |                    |
     |-- OrderCreated -----|-- (process) -------|                    |
     |                     |                    |                    |
     |-- OrderCreated -----|--------------------|----- (email) ---->|
     |                     |                    |                    |
     |<--- OrderPaid ------|                    |                    |
     |                     |                    |                    |
     |--- OrderPaid -------|--------------------|--> (shipping) -----|
     |                     |                    |                    |
     |<--- ShipmentCreated |<-------------------|                    |
     |                     |                    |                    |
     |--- ShipmentCreated -|--------------------|----- (tracking)--->|
```

## 🎭 Real-World Scenarios

### E-commerce Order Processing
```csharp
// Order Service publishes order creation
await eventBus.PublishAsync(new OrderCreatedEvent
{
    OrderId = orderId,
    Items = orderItems,
    CustomerEmail = "customer@store.com"
});

// Multiple services react independently:
// - PaymentService processes payment
// - InventoryService reserves stock  
// - NotificationService sends confirmation email
```

### Cross-Service Event Handlers
```csharp
// Payment Service handles order events
public class PaymentOrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Process payment for the order
        var paymentResult = await ProcessPaymentAsync(@event.OrderId, @event.TotalAmount);
        
        if (paymentResult.Success)
        {
            // Publish payment completed event
            await eventBus.PublishAsync(new OrderPaidEvent
            {
                OrderId = @event.OrderId,
                TransactionId = paymentResult.TransactionId,
                CorrelationId = @event.CorrelationId
            });
        }
    }
}
```

## ⚙️ Configuration Options

```csharp
public class EventBusConfiguration
{
    public string ServiceName { get; set; } = "Unknown";
    public int MaxConcurrentHandlers { get; set; } = 10;
    public int ChannelCapacity { get; set; } = 1000;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxRetryAttempts { get; set; } = 3;
    public bool EnableDeadLetterQueue { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
}
```

## 📊 Monitoring and Metrics

The implementation provides comprehensive metrics:

- **Published Events**: Count by event type
- **Processed Events**: Count and duration by service
- **Failed Events**: Count and error details
- **Dead Letter Queue**: Failed events for manual review
- **Handler Performance**: Processing times per handler

## ⚡ Benefits

- **Loose Coupling**: Services don't need direct knowledge of each other
- **Scalability**: Asynchronous processing supports high throughput
- **Resilience**: Retry logic and dead letter queues handle failures
- **Flexibility**: Easy to add new services and event handlers
- **Observability**: Built-in metrics and monitoring
- **Event Correlation**: Track related events across services

## ⚠️ Considerations

- **Event Ordering**: No guarantee of message ordering across services
- **Eventual Consistency**: Changes propagate asynchronously
- **Error Handling**: Failed events need manual intervention from dead letter queue
- **Schema Evolution**: Event structure changes require careful versioning
- **Testing**: Integration testing across services can be complex

## 🔄 Integration Patterns

Works well with:
- **CQRS**: Commands create events for query model updates
- **Event Sourcing**: Events become the source of truth
- **Saga Pattern**: Orchestrate complex business transactions
- **Circuit Breaker**: Protect against cascading failures
- **Bulk Head**: Isolate different event types

## 🎯 Use Cases

- **Order Processing**: Coordinate multiple services in e-commerce
- **User Registration**: Update multiple systems when users register  
- **Inventory Management**: Sync stock levels across services
- **Notification Systems**: Send alerts based on business events
- **Audit Logging**: Track system-wide activities
- **Data Synchronization**: Keep distributed data stores in sync

This Event Bus implementation provides a production-ready foundation for microservices communication with enterprise-grade reliability and monitoring capabilities.
