using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Threading;

// ============================================================================
// EVENT DRIVEN PROGRAMMING - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de e-commerce con arquitectura dirigida por eventos
// ============================================================================

// ============================================================================
// INFRAESTRUCTURA DE EVENTOS
// ============================================================================

public interface IEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
    string Source { get; }
}

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
    void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
    void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;
}

public interface IEventStore
{
    Task SaveEventAsync(IEvent @event);
    Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId);
    Task<IEnumerable<IEvent>> GetEventsByTypeAsync(string eventType);
}

// ============================================================================
// EVENTOS DE DOMINIO
// ============================================================================

public abstract class BaseEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
    public abstract string Source { get; }
}

// Eventos del dominio de órdenes
public class OrderSubmittedEvent : BaseEvent
{
    public override string EventType => "OrderSubmitted";
    public override string Source => "OrderService";
    
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemData> Items { get; set; } = new();
}

public class OrderValidatedEvent : BaseEvent
{
    public override string EventType => "OrderValidated";
    public override string Source => "OrderService";
    
    public Guid OrderId { get; set; }
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; }
}

public class OrderPaymentProcessedEvent : BaseEvent
{
    public override string EventType => "OrderPaymentProcessed";
    public override string Source => "PaymentService";
    
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public bool Success { get; set; }
    public string TransactionId { get; set; }
}

public class InventoryReservationRequestedEvent : BaseEvent
{
    public override string EventType => "InventoryReservationRequested";
    public override string Source => "OrderService";
    
    public Guid OrderId { get; set; }
    public List<InventoryReservationItem> Items { get; set; } = new();
}

public class InventoryReservedEvent : BaseEvent
{
    public override string EventType => "InventoryReserved";
    public override string Source => "InventoryService";
    
    public Guid OrderId { get; set; }
    public Guid ReservationId { get; set; }
    public List<InventoryReservationItem> ReservedItems { get; set; } = new();
    public bool Success { get; set; }
    public string Message { get; set; }
}

public class OrderFulfilledEvent : BaseEvent
{
    public override string EventType => "OrderFulfilled";
    public override string Source => "OrderService";
    
    public Guid OrderId { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}

public class OrderFailedEvent : BaseEvent
{
    public override string EventType => "OrderFailed";
    public override string Source => "OrderService";
    
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
    public string FailureStage { get; set; }
}

public class NotificationSentEvent : BaseEvent
{
    public override string EventType => "NotificationSent";
    public override string Source => "NotificationService";
    
    public string RecipientEmail { get; set; }
    public string NotificationType { get; set; }
    public string Subject { get; set; }
    public bool Success { get; set; }
}

// ============================================================================
// DATOS DE APOYO
// ============================================================================

public class OrderItemData
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class InventoryReservationItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}

// ============================================================================
// EVENT BUS IMPLEMENTATION
// ============================================================================

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<object>> _handlers = new();
    private readonly Channel<IEvent> _eventChannel;
    private readonly ChannelReader<IEvent> _eventReader;
    private readonly ChannelWriter<IEvent> _eventWriter;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public InMemoryEventBus(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            WaitForSpace = true,
            FullMode = BoundedChannelFullMode.Wait
        };
        
        _eventChannel = Channel.CreateBounded<IEvent>(options);
        _eventReader = _eventChannel.Reader;
        _eventWriter = _eventChannel.Writer;
        
        // Iniciar procesamiento en segundo plano
        _ = Task.Run(ProcessEventsAsync);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await _eventWriter.WriteAsync(@event, cancellationToken);
        Console.WriteLine($"📤 Evento publicado: {@event.EventType} (ID: {@event.Id.ToString()[..8]}...)");
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<object>();
        }
        
        _handlers[eventType].Add(handler);
        Console.WriteLine($"🔗 Handler suscrito para evento: {eventType.Name}");
    }

    public void Unsubscribe<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        
        if (_handlers.ContainsKey(eventType))
        {
            _handlers[eventType].Remove(handler);
            Console.WriteLine($"🔌 Handler desuscrito para evento: {eventType.Name}");
        }
    }

    private async Task ProcessEventsAsync()
    {
        await foreach (var @event in _eventReader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            try
            {
                await ProcessEvent(@event);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error procesando evento {@event.EventType}: {ex.Message}");
            }
        }
    }

    private async Task ProcessEvent(IEvent @event)
    {
        var eventType = @event.GetType();
        
        if (_handlers.ContainsKey(eventType))
        {
            var handlers = _handlers[eventType].Cast<dynamic>().ToList();
            
            Console.WriteLine($"📥 Procesando evento: {@event.EventType} - {handlers.Count} handlers");
            
            var tasks = handlers.Select(async (dynamic handler) =>
            {
                try
                {
                    await handler.HandleAsync((dynamic)@event, _cancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error en handler para {@event.EventType}: {ex.Message}");
                }
            });
            
            await Task.WhenAll(tasks);
        }
    }

    public void Dispose()
    {
        _eventWriter.Complete();
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}

// ============================================================================
// EVENT STORE IMPLEMENTATION
// ============================================================================

public class InMemoryEventStore : IEventStore
{
    private readonly List<IEvent> _events = new();
    private readonly object _lock = new();

    public Task SaveEventAsync(IEvent @event)
    {
        lock (_lock)
        {
            _events.Add(@event);
            Console.WriteLine($"💾 Evento guardado en store: {@event.EventType}");
        }
        
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId)
    {
        lock (_lock)
        {
            // Simulamos que los eventos tienen un campo AggregateId
            var filteredEvents = _events.Where(e => 
                e.GetType().GetProperty("OrderId")?.GetValue(e)?.ToString() == aggregateId ||
                e.GetType().GetProperty("CustomerId")?.GetValue(e)?.ToString() == aggregateId);
            
            return Task.FromResult(filteredEvents);
        }
    }

    public Task<IEnumerable<IEvent>> GetEventsByTypeAsync(string eventType)
    {
        lock (_lock)
        {
            var filteredEvents = _events.Where(e => e.EventType == eventType);
            return Task.FromResult(filteredEvents);
        }
    }

    public int GetTotalEvents()
    {
        lock (_lock)
        {
            return _events.Count;
        }
    }

    public Dictionary<string, int> GetEventStatistics()
    {
        lock (_lock)
        {
            return _events.GroupBy(e => e.EventType)
                         .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}

// ============================================================================
// SERVICIOS DIRIGIDOS POR EVENTOS
// ============================================================================

// Servicio de órdenes que reacciona a eventos
public class OrderEventHandler : 
    IEventHandler<OrderSubmittedEvent>,
    IEventHandler<OrderPaymentProcessedEvent>,
    IEventHandler<InventoryReservedEvent>
{
    private readonly IEventBus _eventBus;
    private readonly IEventStore _eventStore;

    public OrderEventHandler(IEventBus eventBus, IEventStore eventStore)
    {
        _eventBus = eventBus;
        _eventStore = eventStore;
    }

    public async Task HandleAsync(OrderSubmittedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🛒 Procesando orden enviada: {@event.OrderId}");
        
        await _eventStore.SaveEventAsync(@event);
        
        // Simular validación de orden
        await Task.Delay(500, cancellationToken);
        
        var isValid = @event.TotalAmount > 0 && @event.Items.Any();
        
        await _eventBus.PublishAsync(new OrderValidatedEvent
        {
            OrderId = @event.OrderId,
            IsValid = isValid,
            ValidationMessage = isValid ? "Orden válida" : "Orden inválida: datos incompletos"
        }, cancellationToken);

        if (isValid)
        {
            // Solicitar reserva de inventario
            await _eventBus.PublishAsync(new InventoryReservationRequestedEvent
            {
                OrderId = @event.OrderId,
                Items = @event.Items.Select(i => new InventoryReservationItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity
                }).ToList()
            }, cancellationToken);
        }
    }

    public async Task HandleAsync(OrderPaymentProcessedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"💳 Procesando pago de orden: {@event.OrderId} - {(@event.Success ? "Exitoso" : "Fallido")}");
        
        await _eventStore.SaveEventAsync(@event);

        if (@event.Success)
        {
            // Orden completada exitosamente
            await _eventBus.PublishAsync(new OrderFulfilledEvent
            {
                OrderId = @event.OrderId,
                TrackingNumber = $"TRK-{@event.OrderId.ToString()[..8]}",
                EstimatedDelivery = DateTime.UtcNow.AddDays(3)
            }, cancellationToken);
        }
        else
        {
            await _eventBus.PublishAsync(new OrderFailedEvent
            {
                OrderId = @event.OrderId,
                Reason = "Pago rechazado",
                FailureStage = "Payment"
            }, cancellationToken);
        }
    }

    public async Task HandleAsync(InventoryReservedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📦 Procesando reserva de inventario: {@event.OrderId} - {(@event.Success ? "Exitosa" : "Fallida")}");
        
        await _eventStore.SaveEventAsync(@event);

        if (@event.Success)
        {
            // Proceder con el pago
            await _eventBus.PublishAsync(new OrderPaymentProcessedEvent
            {
                OrderId = @event.OrderId,
                PaymentId = Guid.NewGuid(),
                Amount = 150.00m, // Simulado
                Success = new Random().Next(1, 10) > 2, // 80% de éxito
                TransactionId = $"TXN-{Guid.NewGuid().ToString()[..8]}"
            }, cancellationToken);
        }
        else
        {
            await _eventBus.PublishAsync(new OrderFailedEvent
            {
                OrderId = @event.OrderId,
                Reason = @event.Message,
                FailureStage = "Inventory"
            }, cancellationToken);
        }
    }
}

// Servicio de inventario
public class InventoryEventHandler : IEventHandler<InventoryReservationRequestedEvent>
{
    private readonly IEventBus _eventBus;
    private readonly IEventStore _eventStore;
    private readonly Dictionary<Guid, int> _inventory = new();

    public InventoryEventHandler(IEventBus eventBus, IEventStore eventStore)
    {
        _eventBus = eventBus;
        _eventStore = eventStore;
        
        // Inicializar inventario de prueba
        _inventory[Guid.Parse("11111111-1111-1111-1111-111111111111")] = 50; // Laptop
        _inventory[Guid.Parse("22222222-2222-2222-2222-222222222222")] = 100; // Mouse
        _inventory[Guid.Parse("33333333-3333-3333-3333-333333333333")] = 25; // Teclado
    }

    public async Task HandleAsync(InventoryReservationRequestedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📋 Procesando reserva de inventario para orden: {@event.OrderId}");
        
        await _eventStore.SaveEventAsync(@event);
        await Task.Delay(300, cancellationToken); // Simular procesamiento

        var reservedItems = new List<InventoryReservationItem>();
        var success = true;
        var message = "";

        foreach (var item in @event.Items)
        {
            if (_inventory.ContainsKey(item.ProductId) && _inventory[item.ProductId] >= item.Quantity)
            {
                _inventory[item.ProductId] -= item.Quantity;
                reservedItems.Add(item);
                Console.WriteLine($"   ✅ Reservado: {item.ProductName} x{item.Quantity}");
            }
            else
            {
                success = false;
                message = $"Stock insuficiente para {item.ProductName}";
                Console.WriteLine($"   ❌ {message}");
                break;
            }
        }

        if (!success)
        {
            // Liberar items reservados anteriormente
            foreach (var item in reservedItems)
            {
                _inventory[item.ProductId] += item.Quantity;
            }
            reservedItems.Clear();
        }

        await _eventBus.PublishAsync(new InventoryReservedEvent
        {
            OrderId = @event.OrderId,
            ReservationId = Guid.NewGuid(),
            ReservedItems = reservedItems,
            Success = success,
            Message = success ? "Inventario reservado exitosamente" : message
        }, cancellationToken);
    }
}

// Servicio de notificaciones
public class NotificationEventHandler : 
    IEventHandler<OrderFulfilledEvent>,
    IEventHandler<OrderFailedEvent>
{
    private readonly IEventBus _eventBus;
    private readonly IEventStore _eventStore;

    public NotificationEventHandler(IEventBus eventBus, IEventStore eventStore)
    {
        _eventBus = eventBus;
        _eventStore = eventStore;
    }

    public async Task HandleAsync(OrderFulfilledEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📧 Enviando notificación de orden completada: {@event.OrderId}");
        
        await _eventStore.SaveEventAsync(@event);
        await Task.Delay(200, cancellationToken); // Simular envío

        await _eventBus.PublishAsync(new NotificationSentEvent
        {
            RecipientEmail = "customer@example.com",
            NotificationType = "OrderFulfilled",
            Subject = $"Tu orden ha sido enviada - Tracking: {@event.TrackingNumber}",
            Success = true
        }, cancellationToken);
    }

    public async Task HandleAsync(OrderFailedEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📧 Enviando notificación de orden fallida: {@event.OrderId}");
        
        await _eventStore.SaveEventAsync(@event);
        await Task.Delay(200, cancellationToken);

        await _eventBus.PublishAsync(new NotificationSentEvent
        {
            RecipientEmail = "customer@example.com",
            NotificationType = "OrderFailed",
            Subject = $"Problema con tu orden - Razón: {@event.Reason}",
            Success = true
        }, cancellationToken);
    }
}

// Servicio de auditoría que registra todos los eventos
public class AuditEventHandler : 
    IEventHandler<OrderSubmittedEvent>,
    IEventHandler<OrderValidatedEvent>,
    IEventHandler<OrderPaymentProcessedEvent>,
    IEventHandler<InventoryReservedEvent>,
    IEventHandler<OrderFulfilledEvent>,
    IEventHandler<OrderFailedEvent>,
    IEventHandler<NotificationSentEvent>
{
    private readonly IEventStore _eventStore;

    public AuditEventHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task HandleAsync(OrderSubmittedEvent @event, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent(@event, "Orden enviada para procesamiento");
    }

    public async Task HandleAsync(OrderValidatedEvent @event, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent(@event, $"Orden validada: {@event.ValidationMessage}");
    }

    public async Task HandleAsync(OrderPaymentProcessedEvent @event, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent(@event, $"Pago procesado: {@event.TransactionId} - {(@event.Success ? "Exitoso" : "Fallido")}");
    }

    public async Task HandleAsync(InventoryReservedEvent @event, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent(@event, $"Inventario: {@event.Message}");
    }

    public async Task HandleAsync(OrderFulfilledEvent @event, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent(@event, $"Orden completada - Tracking: {@event.TrackingNumber}");
    }

    public async Task HandleAsync(OrderFailedEvent @event, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent(@event, $"Orden fallida en {@event.FailureStage}: {@event.Reason}");
    }

    public async Task HandleAsync(NotificationSentEvent @event, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent(@event, $"Notificación enviada: {@event.NotificationType}");
    }

    private async Task LogAuditEvent(IEvent @event, string description)
    {
        Console.WriteLine($"📊 AUDIT: [{@event.OccurredAt:HH:mm:ss}] {@event.EventType} - {description}");
        await _eventStore.SaveEventAsync(@event);
        await Task.CompletedTask;
    }
}

// ============================================================================
// SERVICIO COORDINADOR
// ============================================================================

public class EventDrivenOrderService
{
    private readonly IEventBus _eventBus;

    public EventDrivenOrderService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task SubmitOrderAsync(Guid customerId, string customerEmail, List<OrderItemData> items)
    {
        var orderId = Guid.NewGuid();
        var totalAmount = items.Sum(i => i.UnitPrice * i.Quantity);

        Console.WriteLine($"\n🚀 Iniciando procesamiento de orden {orderId}");
        Console.WriteLine($"   Cliente: {customerEmail}");
        Console.WriteLine($"   Total: ${totalAmount:F2}");
        Console.WriteLine($"   Items: {items.Count}");

        await _eventBus.PublishAsync(new OrderSubmittedEvent
        {
            OrderId = orderId,
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            TotalAmount = totalAmount,
            Items = items
        });
    }
}

// ============================================================================
// DEMO REALISTA
// ============================================================================

public static class EventDrivenDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🎯 DEMO: EVENT DRIVEN PROGRAMMING - SISTEMA DE E-COMMERCE");
        Console.WriteLine("=========================================================");
        Console.WriteLine("Demostración de arquitectura dirigida por eventos\n");

        // Configurar infraestructura
        var eventBus = new InMemoryEventBus();
        var eventStore = new InMemoryEventStore();

        // Configurar handlers
        var orderHandler = new OrderEventHandler(eventBus, eventStore);
        var inventoryHandler = new InventoryEventHandler(eventBus, eventStore);
        var notificationHandler = new NotificationEventHandler(eventBus, eventStore);
        var auditHandler = new AuditEventHandler(eventStore);

        // Suscribir handlers a eventos
        Console.WriteLine("🔗 CONFIGURANDO SUSCRIPCIONES DE EVENTOS:");
        eventBus.Subscribe<OrderSubmittedEvent>(orderHandler);
        eventBus.Subscribe<OrderPaymentProcessedEvent>(orderHandler);
        eventBus.Subscribe<InventoryReservedEvent>(orderHandler);
        eventBus.Subscribe<InventoryReservationRequestedEvent>(inventoryHandler);
        eventBus.Subscribe<OrderFulfilledEvent>(notificationHandler);
        eventBus.Subscribe<OrderFailedEvent>(notificationHandler);
        
        // Suscribir auditoría a todos los eventos
        eventBus.Subscribe<OrderSubmittedEvent>(auditHandler);
        eventBus.Subscribe<OrderValidatedEvent>(auditHandler);
        eventBus.Subscribe<OrderPaymentProcessedEvent>(auditHandler);
        eventBus.Subscribe<InventoryReservedEvent>(auditHandler);
        eventBus.Subscribe<OrderFulfilledEvent>(auditHandler);
        eventBus.Subscribe<OrderFailedEvent>(auditHandler);
        eventBus.Subscribe<NotificationSentEvent>(auditHandler);

        var orderService = new EventDrivenOrderService(eventBus);

        Console.WriteLine("\n📋 PROCESANDO ÓRDENES:");
        Console.WriteLine("=======================");

        // Procesar múltiples órdenes para mostrar el flujo
        var orders = new[]
        {
            new
            {
                CustomerId = Guid.NewGuid(),
                Email = "cliente1@example.com",
                Items = new List<OrderItemData>
                {
                    new() { ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ProductName = "Laptop Gaming", Quantity = 1, UnitPrice = 999.99m },
                    new() { ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ProductName = "Mouse Gaming", Quantity = 1, UnitPrice = 49.99m }
                }
            },
            new
            {
                CustomerId = Guid.NewGuid(),
                Email = "cliente2@example.com",
                Items = new List<OrderItemData>
                {
                    new() { ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"), ProductName = "Teclado Mecánico", Quantity = 2, UnitPrice = 129.99m }
                }
            },
            new
            {
                CustomerId = Guid.NewGuid(),
                Email = "cliente3@example.com",
                Items = new List<OrderItemData>
                {
                    new() { ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ProductName = "Laptop Gaming", Quantity = 100, UnitPrice = 999.99m } // Esto debería fallar por inventario
                }
            }
        };

        foreach (var order in orders)
        {
            await orderService.SubmitOrderAsync(order.CustomerId, order.Email, order.Items);
            await Task.Delay(2000); // Pausa para ver el flujo de eventos
        }

        // Esperar a que se procesen todos los eventos
        await Task.Delay(3000);

        // Mostrar estadísticas
        Console.WriteLine("\n📊 ESTADÍSTICAS FINALES:");
        Console.WriteLine("========================");
        Console.WriteLine($"Total de eventos procesados: {eventStore.GetTotalEvents()}");
        
        var stats = eventStore.GetEventStatistics();
        foreach (var stat in stats.OrderByDescending(s => s.Value))
        {
            Console.WriteLine($"  • {stat.Key}: {stat.Value} eventos");
        }

        Console.WriteLine("\n✅ Demo completado");
        Console.WriteLine("\n💡 BENEFICIOS DEL EVENT DRIVEN PROGRAMMING:");
        Console.WriteLine("  • Desacoplamiento entre servicios");
        Console.WriteLine("  • Escalabilidad horizontal");
        Console.WriteLine("  • Tolerancia a fallos");
        Console.WriteLine("  • Auditoría completa de eventos");
        Console.WriteLine("  • Facilidad para agregar nuevos servicios");
        Console.WriteLine("  • Procesamiento asíncrono");

        // Cleanup
        if (eventBus is InMemoryEventBus busImpl)
        {
            busImpl.Dispose();
        }
    }
}
