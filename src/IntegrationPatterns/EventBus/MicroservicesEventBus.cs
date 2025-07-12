using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Channels;
using System.Collections.Concurrent;
using System.Text.Json;

// ============================================================================
// MICROSERVICES EVENT BUS PATTERN - IMPLEMENTACIÓN AVANZADA
// Ejemplo: Event Bus distribuido para comunicación entre microservicios
// ============================================================================

// ============================================================================
// EVENT BUS CORE - NÚCLEO DEL EVENT BUS
// ============================================================================

/// <summary>
/// Interfaz para eventos que viajan entre microservicios
/// </summary>
public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
    string Source { get; }
    string CorrelationId { get; }
    Dictionary<string, string> Headers { get; }
}

/// <summary>
/// Evento base para integración entre microservicios
/// </summary>
public abstract class IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
    public abstract string Source { get; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// Handler para eventos de integración
/// </summary>
public interface IIntegrationEventHandler<in TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Event Bus para microservicios distribuidos
/// </summary>
public interface IMicroservicesEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent;
    void Subscribe<T>(IIntegrationEventHandler<T> handler) where T : IIntegrationEvent;
    void Subscribe<T>(string microserviceName, IIntegrationEventHandler<T> handler) where T : IIntegrationEvent;
    void Unsubscribe<T>(IIntegrationEventHandler<T> handler) where T : IIntegrationEvent;
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<EventBusStatistics> GetStatisticsAsync();
}

/// <summary>
/// Configuración del Event Bus
/// </summary>
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

// ============================================================================
// MICROSERVICES EVENT BUS IMPLEMENTATION
// ============================================================================

/// <summary>
/// Implementación del Event Bus para microservicios
/// </summary>
public class MicroservicesEventBus : IMicroservicesEventBus, IDisposable
{
    private readonly EventBusConfiguration _config;
    private readonly Channel<EventEnvelope> _eventChannel;
    private readonly ChannelReader<EventEnvelope> _eventReader;
    private readonly ChannelWriter<EventEnvelope> _eventWriter;
    
    private readonly ConcurrentDictionary<Type, List<HandlerInfo>> _handlers = new();
    private readonly ConcurrentDictionary<string, List<EventEnvelope>> _deadLetterQueue = new();
    private readonly EventBusMetrics _metrics = new();
    
    private readonly List<Task> _workers = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly SemaphoreSlim _concurrencyLimiter;

    public event EventHandler<EventPublishedEventArgs> EventPublished;
    public event EventHandler<EventProcessedEventArgs> EventProcessed;
    public event EventHandler<EventFailedEventArgs> EventFailed;
    public event EventHandler<HandlerRegisteredEventArgs> HandlerRegistered;

    public MicroservicesEventBus(EventBusConfiguration config = null)
    {
        _config = config ?? new EventBusConfiguration();
        
        var channelOptions = new BoundedChannelOptions(_config.ChannelCapacity)
        {
            WaitForSpace = true,
            FullMode = BoundedChannelFullMode.Wait
        };
        
        _eventChannel = Channel.CreateBounded<EventEnvelope>(channelOptions);
        _eventReader = _eventChannel.Reader;
        _eventWriter = _eventChannel.Writer;
        
        _concurrencyLimiter = new SemaphoreSlim(_config.MaxConcurrentHandlers, _config.MaxConcurrentHandlers);

        Console.WriteLine($"🚀 MicroservicesEventBus iniciado para servicio: {_config.ServiceName}");
        Console.WriteLine($"   Configuración: {_config.MaxConcurrentHandlers} handlers concurrentes, canal de {_config.ChannelCapacity} eventos");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // Iniciar workers para procesar eventos
        for (int i = 0; i < _config.MaxConcurrentHandlers; i++)
        {
            var workerId = i + 1;
            _workers.Add(Task.Run(() => ProcessEventsAsync(workerId, _cancellationTokenSource.Token)));
        }

        // Worker para métricas
        if (_config.EnableMetrics)
        {
            _workers.Add(Task.Run(() => LogMetricsAsync(_cancellationTokenSource.Token)));
        }

        Console.WriteLine($"✅ Event Bus iniciado con {_workers.Count} workers");
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _eventWriter.Complete();
        _cancellationTokenSource.Cancel();

        try
        {
            await Task.WhenAll(_workers.ToArray()).WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
        }
        catch (TimeoutException)
        {
            Console.WriteLine("⚠️ Timeout al detener workers del Event Bus");
        }

        Console.WriteLine("🛑 Event Bus detenido");
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent
    {
        var envelope = new EventEnvelope
        {
            Event = @event,
            EventType = typeof(T),
            PublishedAt = DateTime.UtcNow,
            Source = _config.ServiceName,
            CorrelationId = @event.CorrelationId
        };

        await _eventWriter.WriteAsync(envelope, cancellationToken);
        
        _metrics.RecordEventPublished(@event.EventType);
        
        Console.WriteLine($"📤 Evento publicado: {@event.EventType} (ID: {@event.Id.ToString()[..8]}...) por {_config.ServiceName}");
        
        EventPublished?.Invoke(this, new EventPublishedEventArgs
        {
            Event = @event,
            PublishedAt = envelope.PublishedAt,
            Source = _config.ServiceName
        });
    }

    public void Subscribe<T>(IIntegrationEventHandler<T> handler) where T : IIntegrationEvent
    {
        Subscribe(_config.ServiceName, handler);
    }

    public void Subscribe<T>(string microserviceName, IIntegrationEventHandler<T> handler) where T : IIntegrationEvent
    {
        var eventType = typeof(T);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<HandlerInfo>();
        }

        var handlerInfo = new HandlerInfo
        {
            Handler = handler,
            HandlerType = handler.GetType(),
            MicroserviceName = microserviceName,
            RegisteredAt = DateTime.UtcNow
        };

        _handlers[eventType].Add(handlerInfo);
        
        Console.WriteLine($"🔗 Handler registrado: {eventType.Name} -> {handler.GetType().Name} (Servicio: {microserviceName})");
        
        HandlerRegistered?.Invoke(this, new HandlerRegisteredEventArgs
        {
            EventType = eventType.Name,
            HandlerType = handler.GetType().Name,
            MicroserviceName = microserviceName
        });
    }

    public void Unsubscribe<T>(IIntegrationEventHandler<T> handler) where T : IIntegrationEvent
    {
        var eventType = typeof(T);
        
        if (_handlers.ContainsKey(eventType))
        {
            _handlers[eventType].RemoveAll(h => h.Handler.Equals(handler));
            Console.WriteLine($"🔌 Handler desregistrado: {eventType.Name} -> {handler.GetType().Name}");
        }
    }

    private async Task ProcessEventsAsync(int workerId, CancellationToken cancellationToken)
    {
        Console.WriteLine($"👷 Event Worker {workerId} iniciado");

        await foreach (var envelope in _eventReader.ReadAllAsync(cancellationToken))
        {
            await _concurrencyLimiter.WaitAsync(cancellationToken);
            
            try
            {
                await ProcessEventAsync(workerId, envelope, cancellationToken);
            }
            finally
            {
                _concurrencyLimiter.Release();
            }
        }

        Console.WriteLine($"👷 Event Worker {workerId} detenido");
    }

    private async Task ProcessEventAsync(int workerId, EventEnvelope envelope, CancellationToken cancellationToken)
    {
        var @event = envelope.Event;
        var eventType = envelope.EventType;

        Console.WriteLine($"⚡ Worker {workerId} procesando: {@event.EventType} (ID: {@event.Id.ToString()[..8]}...)");

        if (_handlers.ContainsKey(eventType))
        {
            var handlers = _handlers[eventType].ToList();
            
            Console.WriteLine($"   Ejecutando {handlers.Count} handlers");

            var tasks = handlers.Select(async handlerInfo =>
            {
                var startTime = DateTime.UtcNow;
                
                try
                {
                    await ExecuteHandlerAsync(handlerInfo, @event, cancellationToken);
                    
                    var duration = DateTime.UtcNow - startTime;
                    _metrics.RecordEventProcessed(@event.EventType, handlerInfo.MicroserviceName, duration);
                    
                    Console.WriteLine($"   ✅ Handler {handlerInfo.HandlerType.Name} completado en {duration.TotalMilliseconds:F0}ms");
                    
                    EventProcessed?.Invoke(this, new EventProcessedEventArgs
                    {
                        Event = @event,
                        HandlerType = handlerInfo.HandlerType.Name,
                        MicroserviceName = handlerInfo.MicroserviceName,
                        Duration = duration,
                        WorkerId = workerId
                    });
                }
                catch (Exception ex)
                {
                    var duration = DateTime.UtcNow - startTime;
                    await HandleEventFailureAsync(envelope, handlerInfo, ex);
                    
                    _metrics.RecordEventFailed(@event.EventType, handlerInfo.MicroserviceName, ex);
                    
                    Console.WriteLine($"   ❌ Handler {handlerInfo.HandlerType.Name} falló: {ex.Message}");
                    
                    EventFailed?.Invoke(this, new EventFailedEventArgs
                    {
                        Event = @event,
                        HandlerType = handlerInfo.HandlerType.Name,
                        MicroserviceName = handlerInfo.MicroserviceName,
                        Exception = ex,
                        WorkerId = workerId
                    });
                }
            });

            await Task.WhenAll(tasks);
        }
        else
        {
            Console.WriteLine($"   ⚠️ No hay handlers registrados para {eventType.Name}");
        }
    }

    private async Task ExecuteHandlerAsync(HandlerInfo handlerInfo, IIntegrationEvent @event, CancellationToken cancellationToken)
    {
        // Usar reflexión para ejecutar el handler específico
        var handlerType = handlerInfo.Handler.GetType();
        var handleMethod = handlerType.GetMethod("HandleAsync");
        
        if (handleMethod != null)
        {
            var result = handleMethod.Invoke(handlerInfo.Handler, new object[] { @event, cancellationToken });
            
            if (result is Task task)
            {
                await task;
            }
        }
    }

    private async Task HandleEventFailureAsync(EventEnvelope envelope, HandlerInfo handlerInfo, Exception exception)
    {
        envelope.AttemptCount++;
        envelope.LastError = exception.Message;
        envelope.LastAttemptAt = DateTime.UtcNow;

        if (envelope.AttemptCount < _config.MaxRetryAttempts)
        {
            // Reenviar después de un delay
            var delay = TimeSpan.FromSeconds(_config.RetryDelay.TotalSeconds * Math.Pow(2, envelope.AttemptCount - 1));
            
            Console.WriteLine($"🔄 Reintentando evento {envelope.Event.EventType} en {delay.TotalSeconds:F0} segundos (intento {envelope.AttemptCount}/{_config.MaxRetryAttempts})");
            
            _ = Task.Run(async () =>
            {
                await Task.Delay(delay);
                await _eventWriter.WriteAsync(envelope);
            });
        }
        else if (_config.EnableDeadLetterQueue)
        {
            // Mover a Dead Letter Queue
            var key = $"{envelope.Event.EventType}:{handlerInfo.HandlerType.Name}";
            
            if (!_deadLetterQueue.ContainsKey(key))
            {
                _deadLetterQueue[key] = new List<EventEnvelope>();
            }
            
            _deadLetterQueue[key].Add(envelope);
            Console.WriteLine($"💀 Evento movido a Dead Letter Queue: {envelope.Event.EventType} (Handler: {handlerInfo.HandlerType.Name})");
        }
    }

    private async Task LogMetricsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            
            var stats = _metrics.GetStatistics();
            Console.WriteLine($"📊 Métricas Event Bus: {stats.TotalPublished} publicados, {stats.TotalProcessed} procesados, {stats.TotalFailed} fallidos");
        }
    }

    public Task<EventBusStatistics> GetStatisticsAsync()
    {
        var stats = _metrics.GetStatistics();
        stats.ActiveHandlers = _handlers.Values.SelectMany(h => h).Count();
        stats.DeadLetterQueueSize = _deadLetterQueue.Values.SelectMany(q => q).Count();
        
        return Task.FromResult(stats);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _eventWriter.Complete();
        _concurrencyLimiter?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}

// ============================================================================
// SUPPORTING CLASSES - CLASES DE APOYO
// ============================================================================

public class EventEnvelope
{
    public IIntegrationEvent Event { get; set; }
    public Type EventType { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Source { get; set; }
    public string CorrelationId { get; set; }
    public int AttemptCount { get; set; } = 0;
    public DateTime? LastAttemptAt { get; set; }
    public string LastError { get; set; }
}

public class HandlerInfo
{
    public object Handler { get; set; }
    public Type HandlerType { get; set; }
    public string MicroserviceName { get; set; }
    public DateTime RegisteredAt { get; set; }
}

public class EventBusMetrics
{
    private readonly ConcurrentDictionary<string, int> _publishedEvents = new();
    private readonly ConcurrentDictionary<string, int> _processedEvents = new();
    private readonly ConcurrentDictionary<string, int> _failedEvents = new();
    private readonly ConcurrentDictionary<string, List<TimeSpan>> _processingTimes = new();

    public void RecordEventPublished(string eventType)
    {
        _publishedEvents.AddOrUpdate(eventType, 1, (k, v) => v + 1);
    }

    public void RecordEventProcessed(string eventType, string handlerService, TimeSpan duration)
    {
        var key = $"{eventType}:{handlerService}";
        _processedEvents.AddOrUpdate(key, 1, (k, v) => v + 1);
        
        if (!_processingTimes.ContainsKey(key))
        {
            _processingTimes[key] = new List<TimeSpan>();
        }
        _processingTimes[key].Add(duration);
    }

    public void RecordEventFailed(string eventType, string handlerService, Exception exception)
    {
        var key = $"{eventType}:{handlerService}";
        _failedEvents.AddOrUpdate(key, 1, (k, v) => v + 1);
    }

    public EventBusStatistics GetStatistics()
    {
        return new EventBusStatistics
        {
            TotalPublished = _publishedEvents.Values.Sum(),
            TotalProcessed = _processedEvents.Values.Sum(),
            TotalFailed = _failedEvents.Values.Sum(),
            EventTypeStats = _publishedEvents.ToDictionary(kv => kv.Key, kv => new EventTypeStatistics
            {
                Published = kv.Value,
                Processed = _processedEvents.Where(p => p.Key.StartsWith(kv.Key)).Sum(p => p.Value),
                Failed = _failedEvents.Where(f => f.Key.StartsWith(kv.Key)).Sum(f => f.Value)
            })
        };
    }
}

public class EventBusStatistics
{
    public int TotalPublished { get; set; }
    public int TotalProcessed { get; set; }
    public int TotalFailed { get; set; }
    public int ActiveHandlers { get; set; }
    public int DeadLetterQueueSize { get; set; }
    public Dictionary<string, EventTypeStatistics> EventTypeStats { get; set; } = new();
}

public class EventTypeStatistics
{
    public int Published { get; set; }
    public int Processed { get; set; }
    public int Failed { get; set; }
}

// ============================================================================
// EVENT ARGS - ARGUMENTOS DE EVENTOS
// ============================================================================

public class EventPublishedEventArgs : EventArgs
{
    public IIntegrationEvent Event { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Source { get; set; }
}

public class EventProcessedEventArgs : EventArgs
{
    public IIntegrationEvent Event { get; set; }
    public string HandlerType { get; set; }
    public string MicroserviceName { get; set; }
    public TimeSpan Duration { get; set; }
    public int WorkerId { get; set; }
}

public class EventFailedEventArgs : EventArgs
{
    public IIntegrationEvent Event { get; set; }
    public string HandlerType { get; set; }
    public string MicroserviceName { get; set; }
    public Exception Exception { get; set; }
    public int WorkerId { get; set; }
}

public class HandlerRegisteredEventArgs : EventArgs
{
    public string EventType { get; set; }
    public string HandlerType { get; set; }
    public string MicroserviceName { get; set; }
}

// ============================================================================
// INTEGRATION EVENTS - EVENTOS DE INTEGRACIÓN
// ============================================================================

public class OrderCreatedEvent : IntegrationEvent
{
    public override string EventType => "OrderCreated";
    public override string Source => "OrderService";
    
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string CustomerEmail { get; set; }
}

public class OrderPaidEvent : IntegrationEvent
{
    public override string EventType => "OrderPaid";
    public override string Source => "PaymentService";
    
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionId { get; set; }
}

public class InventoryReservedEvent : IntegrationEvent
{
    public override string EventType => "InventoryReserved";
    public override string Source => "InventoryService";
    
    public Guid OrderId { get; set; }
    public List<InventoryItemDto> ReservedItems { get; set; } = new();
    public Guid ReservationId { get; set; }
}

public class ShipmentCreatedEvent : IntegrationEvent
{
    public override string EventType => "ShipmentCreated";
    public override string Source => "ShippingService";
    
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime EstimatedDelivery { get; set; }
    public string ShippingAddress { get; set; }
}

public class EmailSentEvent : IntegrationEvent
{
    public override string EventType => "EmailSent";
    public override string Source => "NotificationService";
    
    public string RecipientEmail { get; set; }
    public string Subject { get; set; }
    public string EmailType { get; set; }
    public bool Success { get; set; }
}

// ============================================================================
// DTOs - OBJETOS DE TRANSFERENCIA
// ============================================================================

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class InventoryItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int ReservedQuantity { get; set; }
}

// ============================================================================
// MICROSERVICE HANDLERS - HANDLERS POR MICROSERVICIO
// ============================================================================

// Handlers del Payment Service
public class PaymentOrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"💳 PaymentService: Procesando pago para orden {@event.OrderId}");
        await Task.Delay(1000, cancellationToken); // Simular procesamiento
        
        // Simular procesamiento de pago exitoso (80% de éxito)
        var success = new Random().Next(1, 10) <= 8;
        
        if (success)
        {
            Console.WriteLine($"✅ PaymentService: Pago completado para orden {@event.OrderId}");
        }
        else
        {
            throw new Exception("Pago rechazado por el banco");
        }
    }
}

// Handlers del Inventory Service
public class InventoryOrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📦 InventoryService: Reservando inventario para orden {@event.OrderId}");
        await Task.Delay(800, cancellationToken);
        
        Console.WriteLine($"✅ InventoryService: Inventario reservado para orden {@event.OrderId}");
    }
}

// Handlers del Shipping Service
public class ShippingOrderPaidHandler : IIntegrationEventHandler<OrderPaidEvent>
{
    public async Task HandleAsync(OrderPaidEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🚚 ShippingService: Creando envío para orden {@event.OrderId}");
        await Task.Delay(1200, cancellationToken);
        
        Console.WriteLine($"✅ ShippingService: Envío creado para orden {@event.OrderId}");
    }
}

// Handlers del Notification Service
public class NotificationOrderCreatedHandler : IIntegrationEventHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📧 NotificationService: Enviando confirmación de orden a {@event.CustomerEmail}");
        await Task.Delay(500, cancellationToken);
        
        Console.WriteLine($"✅ NotificationService: Email de confirmación enviado");
    }
}

public class NotificationShipmentCreatedHandler : IIntegrationEventHandler<ShipmentCreatedEvent>
{
    public async Task HandleAsync(ShipmentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📧 NotificationService: Enviando notificación de envío para orden {@event.OrderId}");
        await Task.Delay(400, cancellationToken);
        
        Console.WriteLine($"✅ NotificationService: Notificación de envío enviada - Tracking: {@event.TrackingNumber}");
    }
}

// ============================================================================
// DEMO REALISTA DEL MICROSERVICES EVENT BUS
// ============================================================================

public static class MicroservicesEventBusDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🎯 DEMO: MICROSERVICES EVENT BUS");
        Console.WriteLine("=================================");
        Console.WriteLine("Demostración de comunicación entre microservicios vía eventos\n");

        // Configurar Event Bus para cada microservicio
        var configs = new Dictionary<string, EventBusConfiguration>
        {
            ["OrderService"] = new() { ServiceName = "OrderService", MaxConcurrentHandlers = 5 },
            ["PaymentService"] = new() { ServiceName = "PaymentService", MaxConcurrentHandlers = 3 },
            ["InventoryService"] = new() { ServiceName = "InventoryService", MaxConcurrentHandlers = 4 },
            ["ShippingService"] = new() { ServiceName = "ShippingService", MaxConcurrentHandlers = 2 },
            ["NotificationService"] = new() { ServiceName = "NotificationService", MaxConcurrentHandlers = 6 }
        };

        var eventBuses = new Dictionary<string, IMicroservicesEventBus>();

        // Crear e iniciar Event Buses
        foreach (var config in configs)
        {
            var eventBus = new MicroservicesEventBus(config.Value);
            await eventBus.StartAsync();
            eventBuses[config.Key] = eventBus;
        }

        Console.WriteLine("🔗 REGISTRANDO HANDLERS ENTRE MICROSERVICIOS:");
        Console.WriteLine("=============================================");

        // Registrar handlers cross-service
        
        // PaymentService escucha OrderCreated
        eventBuses["PaymentService"].Subscribe("PaymentService", new PaymentOrderCreatedHandler());
        
        // InventoryService escucha OrderCreated
        eventBuses["InventoryService"].Subscribe("InventoryService", new InventoryOrderCreatedHandler());
        
        // ShippingService escucha OrderPaid
        eventBuses["ShippingService"].Subscribe("ShippingService", new ShippingOrderPaidHandler());
        
        // NotificationService escucha múltiples eventos
        eventBuses["NotificationService"].Subscribe("NotificationService", new NotificationOrderCreatedHandler());
        eventBuses["NotificationService"].Subscribe("NotificationService", new NotificationShipmentCreatedHandler());

        Console.WriteLine("\n🛒 PROCESANDO ÓRDENES:");
        Console.WriteLine("======================");

        // Crear órdenes de prueba
        var orders = new[]
        {
            new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "cliente1@example.com",
                TotalAmount = 299.99m,
                Items = new List<OrderItemDto>
                {
                    new() { ProductId = Guid.NewGuid(), ProductName = "Smartphone", Quantity = 1, UnitPrice = 299.99m }
                }
            },
            new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "cliente2@example.com",
                TotalAmount = 1299.99m,
                Items = new List<OrderItemDto>
                {
                    new() { ProductId = Guid.NewGuid(), ProductName = "Laptop", Quantity = 1, UnitPrice = 1299.99m }
                }
            }
        };

        // Procesar órdenes
        foreach (var order in orders)
        {
            Console.WriteLine($"\n📝 Procesando orden {order.OrderId}:");
            Console.WriteLine($"   Cliente: {order.CustomerEmail}");
            Console.WriteLine($"   Total: ${order.TotalAmount:F2}");

            // OrderService publica OrderCreated
            await eventBuses["OrderService"].PublishAsync(order);

            // Esperar procesamiento
            await Task.Delay(3000);

            // Simular pago exitoso y publicar OrderPaid
            var orderPaid = new OrderPaidEvent
            {
                OrderId = order.OrderId,
                Amount = order.TotalAmount,
                PaymentMethod = "Credit Card",
                TransactionId = $"TXN-{Guid.NewGuid().ToString()[..8]}",
                CorrelationId = order.CorrelationId
            };

            await eventBuses["PaymentService"].PublishAsync(orderPaid);

            // Esperar procesamiento
            await Task.Delay(2000);

            // Simular creación de envío
            var shipmentCreated = new ShipmentCreatedEvent
            {
                OrderId = order.OrderId,
                TrackingNumber = $"TRK-{Guid.NewGuid().ToString()[..8]}",
                EstimatedDelivery = DateTime.UtcNow.AddDays(3),
                ShippingAddress = "Dirección de entrega",
                CorrelationId = order.CorrelationId
            };

            await eventBuses["ShippingService"].PublishAsync(shipmentCreated);

            await Task.Delay(1000);
        }

        Console.WriteLine("\n📊 ESTADÍSTICAS DE EVENT BUSES:");
        Console.WriteLine("===============================");

        foreach (var kvp in eventBuses)
        {
            var stats = await kvp.Value.GetStatisticsAsync();
            Console.WriteLine($"\n{kvp.Key}:");
            Console.WriteLine($"  📤 Publicados: {stats.TotalPublished}");
            Console.WriteLine($"  ✅ Procesados: {stats.TotalProcessed}");
            Console.WriteLine($"  ❌ Fallidos: {stats.TotalFailed}");
            Console.WriteLine($"  🔗 Handlers activos: {stats.ActiveHandlers}");
            
            if (stats.DeadLetterQueueSize > 0)
            {
                Console.WriteLine($"  💀 Dead Letter Queue: {stats.DeadLetterQueueSize}");
            }
        }

        Console.WriteLine("\n⏳ Esperando finalización de procesamiento...");
        await Task.Delay(3000);

        Console.WriteLine("\n🛑 DETENIENDO MICROSERVICIOS:");
        Console.WriteLine("==============================");

        foreach (var kvp in eventBuses)
        {
            await kvp.Value.StopAsync();
            kvp.Value.Dispose();
        }

        Console.WriteLine("\n💡 BENEFICIOS DEL MICROSERVICES EVENT BUS:");
        Console.WriteLine("  • Desacoplamiento total entre microservicios");
        Console.WriteLine("  • Comunicación asíncrona y escalable");
        Console.WriteLine("  • Tolerancia a fallos con reintentos automáticos");
        Console.WriteLine("  • Dead Letter Queue para eventos problemáticos");
        Console.WriteLine("  • Métricas y observabilidad integradas");
        Console.WriteLine("  • Soporte para correlación de eventos");
        Console.WriteLine("  • Procesamiento concurrente configurable");
    }
}
