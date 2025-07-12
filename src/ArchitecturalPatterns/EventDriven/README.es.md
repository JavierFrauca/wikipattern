# Event Driven Programming/Architecture - Programación Dirigida por Eventos

## 📋 Descripción

La **Programación Dirigida por Eventos** es un paradigma de programación donde el flujo del programa está determinado por eventos como acciones del usuario, mensajes de otros programas o threads, o cambios de estado del sistema. En arquitecturas de microservicios, este patrón permite crear sistemas altamente desacoplados y escalables.

## 🎯 Propósito

### Objetivos Principales:
- **Desacoplamiento**: Los servicios no dependen directamente unos de otros
- **Escalabilidad**: Cada servicio puede escalar independientemente
- **Resilencia**: Un fallo en un servicio no afecta a otros
- **Extensibilidad**: Fácil agregar nuevos servicios reactivos
- **Auditoría**: Rastro completo de todos los eventos del sistema

## 🏗️ Estructura del Patrón

### Componentes Clave:

1. **Event**: Representación de algo que ha ocurrido
2. **Event Bus**: Mecanismo de publicación y suscripción
3. **Event Handler**: Componente que reacciona a eventos específicos
4. **Event Store**: Persistencia de eventos para auditoría/replay
5. **Event Publisher**: Quien publica eventos al bus

## 💻 Implementación en C#

### Estructura Básica

```csharp
// Interfaz base para eventos
public interface IEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
    string Source { get; }
}

// Handler para procesar eventos
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

// Bus de eventos para pub/sub
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
}
```

### Eventos de Dominio

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

### Handlers de Eventos

```csharp
public class OrderEventHandler : 
    IEventHandler<OrderSubmittedEvent>,
    IEventHandler<OrderPaymentProcessedEvent>
{
    private readonly IEventBus _eventBus;

    public async Task HandleAsync(OrderSubmittedEvent @event, CancellationToken cancellationToken = default)
    {
        // Validar orden
        var isValid = ValidateOrder(@event);
        
        // Publicar resultado de validación
        await _eventBus.PublishAsync(new OrderValidatedEvent
        {
            OrderId = @event.OrderId,
            IsValid = isValid
        });
        
        if (isValid)
        {
            // Solicitar procesamiento de pago
            await _eventBus.PublishAsync(new PaymentRequestedEvent
            {
                OrderId = @event.OrderId,
                Amount = @event.TotalAmount
            });
        }
    }
}
```

## 🔄 Flujo del Patrón

### Proceso Típico de E-commerce:

1. **Cliente envía orden** → `OrderSubmittedEvent`
2. **Sistema valida orden** → `OrderValidatedEvent`
3. **Reserva inventario** → `InventoryReservedEvent`
4. **Procesa pago** → `PaymentProcessedEvent`
5. **Completa orden** → `OrderFulfilledEvent`
6. **Envía notificación** → `NotificationSentEvent`

### Diagrama de Flujo:

```
[Cliente] → [OrderService] → [EventBus] → [InventoryService]
                ↓                              ↓
           [EventStore]                [PaymentService]
                ↓                              ↓
        [AuditService] ← [EventBus] ← [NotificationService]
```

## ✅ Ventajas

### 🚀 **Escalabilidad**
- Cada servicio se escala independientemente
- Procesamiento asíncrono mejora rendimiento
- Distribución natural de carga

### 🔧 **Mantenibilidad**
- Servicios altamente desacoplados
- Fácil agregar nuevas funcionalidades
- Cambios localizados en cada servicio

### 🛡️ **Resilencia**
- Tolerancia a fallos de servicios individuales
- Capacidad de replay de eventos
- Recuperación automática

### 📊 **Observabilidad**
- Trazabilidad completa de eventos
- Auditoría natural del sistema
- Debugging facilitado

## ⚠️ Desventajas

### 🔄 **Complejidad**
- Lógica distribuida difícil de debuggear
- Eventual consistency
- Manejo de fallos más complejo

### 📈 **Overhead**
- Latencia adicional por comunicación asíncrona
- Infraestructura más compleja
- Monitoreo más sofisticado

### 🎯 **Consistencia**
- No hay transacciones ACID distribuidas
- Posibles estados intermedios inconsistentes
- Necesidad de compensating actions

## 🎮 Casos de Uso Ideales

### ✅ **Recomendado para:**
- **E-commerce**: Procesamiento de órdenes complejas
- **Banking**: Transferencias y transacciones
- **IoT**: Procesamiento de telemetría en tiempo real
- **Gaming**: Sistemas de matchmaking y eventos de juego
- **Logistics**: Tracking de envíos y entregas

### ❌ **No recomendado para:**
- Aplicaciones simples con pocos componentes
- Sistemas que requieren consistencia inmediata
- Operaciones síncronas críticas
- Equipos sin experiencia en sistemas distribuidos

## 🔧 Consideraciones de Implementación

### Event Store
```csharp
public interface IEventStore
{
    Task SaveEventAsync(IEvent @event);
    Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId);
    Task<IEnumerable<IEvent>> GetEventsByTypeAsync(string eventType);
}
```

### Garantías de Entrega
- **At Least Once**: Evento puede procesarse múltiples veces
- **At Most Once**: Evento se procesa máximo una vez
- **Exactly Once**: Evento se procesa exactamente una vez (más difícil)

### Idempotencia
```csharp
public class IdempotentEventHandler : IEventHandler<OrderSubmittedEvent>
{
    private readonly HashSet<Guid> _processedEvents = new();

    public async Task HandleAsync(OrderSubmittedEvent @event, CancellationToken cancellationToken = default)
    {
        if (_processedEvents.Contains(@event.Id))
        {
            Console.WriteLine($"Evento ya procesado: {@event.Id}");
            return; // Skip processing
        }

        // Process event
        await ProcessOrder(@event);
        
        _processedEvents.Add(@event.Id);
    }
}
```

## 📊 Métricas y Monitoreo

### KPIs Importantes:
- **Event Throughput**: Eventos procesados por segundo
- **Processing Latency**: Tiempo de procesamiento promedio
- **Failed Events**: Porcentaje de eventos fallidos
- **Queue Depth**: Profundidad de colas de eventos
- **Handler Response Time**: Tiempo de respuesta por handler

### Monitoreo de Ejemplo:
```csharp
public class EventMetrics
{
    public void RecordEventProcessed(string eventType, TimeSpan duration)
    {
        Console.WriteLine($"📊 Métrica: {eventType} procesado en {duration.TotalMilliseconds}ms");
        // Enviar a sistema de métricas (Prometheus, etc.)
    }
    
    public void RecordEventFailed(string eventType, Exception exception)
    {
        Console.WriteLine($"❌ Error: {eventType} falló - {exception.Message}");
        // Enviar a sistema de alertas
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

## 🔄 Relación con Otros Patrones

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

## 🚀 Ejemplo Práctico

El código incluye un ejemplo completo de un sistema de e-commerce que demuestra:

1. **Procesamiento de órdenes**: Desde envío hasta entrega
2. **Múltiples servicios**: Order, Inventory, Payment, Notification
3. **Manejo de fallos**: Compensación automática
4. **Auditoría**: Tracking completo de eventos
5. **Escalabilidad**: Procesamiento asíncrono
6. **Monitoring**: Métricas en tiempo real

### Ejecutar el Demo:
```bash
dotnet run
```

## 📚 Recursos Adicionales

### Librerías Recomendadas:
- **MassTransit**: Service bus para .NET
- **NServiceBus**: Framework de messaging
- **EventStore**: Database específico para eventos
- **Apache Kafka**: Streaming platform distribuida
- **RabbitMQ**: Message broker
- **Azure Service Bus**: Messaging cloud service

### Patrones Relacionados:
- **Event Sourcing**: Persistir eventos como fuente de verdad
- **CQRS**: Separar lecturas y escrituras
- **Saga Pattern**: Coordinar transacciones distribuidas
- **Outbox Pattern**: Garantizar entrega de eventos
- **Materialized Views**: Vistas optimizadas desde eventos

---

## 📈 Conclusión

Event Driven Programming es fundamental para arquitecturas modernas de microservicios. Permite crear sistemas altamente escalables y resilientes, aunque requiere mayor complejidad en el diseño y operación. Es ideal para dominios complejos donde el desacoplamiento y la escalabilidad son prioritarios.

**Consideración clave**: Implementar gradualmente, comenzando con casos de uso simples y evolucionando hacia workflows más complejos conforme el equipo gana experiencia.
