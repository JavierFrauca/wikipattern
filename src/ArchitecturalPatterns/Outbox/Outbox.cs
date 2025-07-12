using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

// ============================================================================
// OUTBOX PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de e-commerce con garantía de entrega de eventos
// ============================================================================

// ============================================================================
// INFRAESTRUCTURA BASE
// ============================================================================

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
    string AggregateId { get; }
    int Version { get; }
}

public interface IOutboxEvent
{
    Guid Id { get; }
    string EventType { get; }
    string EventData { get; }
    string AggregateId { get; }
    DateTime CreatedAt { get; }
    bool IsProcessed { get; }
    DateTime? ProcessedAt { get; }
    int RetryCount { get; }
    string ErrorMessage { get; }
}

public interface IEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent);
}

public interface IOutboxRepository
{
    Task SaveEventAsync(IOutboxEvent outboxEvent);
    Task<IEnumerable<IOutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 100);
    Task MarkAsProcessedAsync(Guid eventId);
    Task MarkAsFailedAsync(Guid eventId, string errorMessage);
    Task IncrementRetryCountAsync(Guid eventId);
}

public interface IUnitOfWork
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task SaveChangesAsync();
}

// ============================================================================
// EVENTOS DE DOMINIO
// ============================================================================

public class OrderCreatedEvent : IDomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderCreated";
    public string AggregateId { get; set; }
    public int Version { get; set; }
    
    // Datos específicos del evento
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public DateTime OrderDate { get; set; }
}

public class PaymentProcessedEvent : IDomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "PaymentProcessed";
    public string AggregateId { get; set; }
    public int Version { get; set; }
    
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionId { get; set; }
    public PaymentStatus Status { get; set; }
}

public class InventoryReservedEvent : IDomainEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "InventoryReserved";
    public string AggregateId { get; set; }
    public int Version { get; set; }
    
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public int RemainingStock { get; set; }
}

// ============================================================================
// MODELOS DE DOMINIO
// ============================================================================

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public int Version { get; set; }
    
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    public void CreateOrder(Guid customerId, string customerEmail, List<OrderItem> items)
    {
        Status = OrderStatus.Pending;
        TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity);
        
        var orderCreatedEvent = new OrderCreatedEvent
        {
            AggregateId = Id.ToString(),
            Version = Version + 1,
            OrderId = Id,
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            TotalAmount = TotalAmount,
            Items = items,
            OrderDate = OrderDate
        };
        
        AddDomainEvent(orderCreatedEvent);
        Version++;
    }
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Shipped,
    Delivered,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

// ============================================================================
// IMPLEMENTACIÓN DEL OUTBOX
// ============================================================================

public class OutboxEvent : IOutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; }
    public string EventData { get; set; }
    public string AggregateId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string ErrorMessage { get; set; }
}

// Simulación de repositorio de outbox (en memoria para el demo)
public class InMemoryOutboxRepository : IOutboxRepository
{
    private readonly List<OutboxEvent> _outboxEvents = new List<OutboxEvent>();
    private readonly object _lock = new object();

    public Task SaveEventAsync(IOutboxEvent outboxEvent)
    {
        lock (_lock)
        {
            var existingEvent = _outboxEvents.FirstOrDefault(e => e.Id == outboxEvent.Id);
            if (existingEvent == null)
            {
                _outboxEvents.Add(new OutboxEvent
                {
                    Id = outboxEvent.Id,
                    EventType = outboxEvent.EventType,
                    EventData = outboxEvent.EventData,
                    AggregateId = outboxEvent.AggregateId,
                    CreatedAt = outboxEvent.CreatedAt,
                    IsProcessed = outboxEvent.IsProcessed,
                    ProcessedAt = outboxEvent.ProcessedAt,
                    RetryCount = outboxEvent.RetryCount,
                    ErrorMessage = outboxEvent.ErrorMessage
                });
                
                Console.WriteLine($"📦 Evento guardado en Outbox: {outboxEvent.EventType} (ID: {outboxEvent.Id})");
            }
        }
        
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IOutboxEvent>> GetUnprocessedEventsAsync(int batchSize = 100)
    {
        lock (_lock)
        {
            var unprocessedEvents = _outboxEvents
                .Where(e => !e.IsProcessed && e.RetryCount < 3)
                .OrderBy(e => e.CreatedAt)
                .Take(batchSize)
                .Cast<IOutboxEvent>()
                .ToList();
                
            return Task.FromResult<IEnumerable<IOutboxEvent>>(unprocessedEvents);
        }
    }

    public Task MarkAsProcessedAsync(Guid eventId)
    {
        lock (_lock)
        {
            var outboxEvent = _outboxEvents.FirstOrDefault(e => e.Id == eventId);
            if (outboxEvent != null)
            {
                outboxEvent.IsProcessed = true;
                outboxEvent.ProcessedAt = DateTime.UtcNow;
                Console.WriteLine($"✅ Evento marcado como procesado: {eventId}");
            }
        }
        
        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(Guid eventId, string errorMessage)
    {
        lock (_lock)
        {
            var outboxEvent = _outboxEvents.FirstOrDefault(e => e.Id == eventId);
            if (outboxEvent != null)
            {
                outboxEvent.ErrorMessage = errorMessage;
                Console.WriteLine($"❌ Evento marcado como fallido: {eventId} - {errorMessage}");
            }
        }
        
        return Task.CompletedTask;
    }

    public Task IncrementRetryCountAsync(Guid eventId)
    {
        lock (_lock)
        {
            var outboxEvent = _outboxEvents.FirstOrDefault(e => e.Id == eventId);
            if (outboxEvent != null)
            {
                outboxEvent.RetryCount++;
                Console.WriteLine($"🔄 Incrementando retry count para evento {eventId}: {outboxEvent.RetryCount}");
            }
        }
        
        return Task.CompletedTask;
    }

    public int GetTotalEvents()
    {
        lock (_lock)
        {
            return _outboxEvents.Count;
        }
    }

    public int GetProcessedEvents()
    {
        lock (_lock)
        {
            return _outboxEvents.Count(e => e.IsProcessed);
        }
    }
}

// ============================================================================
// PUBLISHER DE EVENTOS
// ============================================================================

public class MessageBusEventPublisher : IEventPublisher
{
    private readonly Random _random = new Random();

    public async Task PublishAsync(IDomainEvent domainEvent)
    {
        // Simular latencia de red
        await Task.Delay(100);
        
        // Simular fallo ocasional para demostrar el retry
        if (_random.Next(1, 10) == 1) // 10% de probabilidad de fallo
        {
            throw new InvalidOperationException("Error temporal del message bus");
        }
        
        Console.WriteLine($"📡 Evento publicado al message bus: {domainEvent.EventType}");
        Console.WriteLine($"   - ID: {domainEvent.Id}");
        Console.WriteLine($"   - Aggregate: {domainEvent.AggregateId}");
        Console.WriteLine($"   - Timestamp: {domainEvent.OccurredAt:yyyy-MM-dd HH:mm:ss}");
    }
}

// ============================================================================
// SERVICIO OUTBOX
// ============================================================================

public class OutboxService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IEventPublisher _eventPublisher;

    public OutboxService(IOutboxRepository outboxRepository, IEventPublisher eventPublisher)
    {
        _outboxRepository = outboxRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task SaveDomainEventAsync(IDomainEvent domainEvent)
    {
        var outboxEvent = new OutboxEvent
        {
            Id = domainEvent.Id,
            EventType = domainEvent.EventType,
            EventData = JsonSerializer.Serialize(domainEvent),
            AggregateId = domainEvent.AggregateId,
            CreatedAt = domainEvent.OccurredAt
        };

        await _outboxRepository.SaveEventAsync(outboxEvent);
    }

    public async Task ProcessPendingEventsAsync()
    {
        Console.WriteLine("\n🔄 Procesando eventos pendientes del Outbox...");
        
        var unprocessedEvents = await _outboxRepository.GetUnprocessedEventsAsync(10);
        
        if (!unprocessedEvents.Any())
        {
            Console.WriteLine("   ℹ️ No hay eventos pendientes");
            return;
        }

        Console.WriteLine($"   📋 Encontrados {unprocessedEvents.Count()} eventos pendientes");

        foreach (var outboxEvent in unprocessedEvents)
        {
            try
            {
                // Deserializar el evento basado en su tipo
                var domainEvent = DeserializeDomainEvent(outboxEvent);
                
                if (domainEvent != null)
                {
                    await _eventPublisher.PublishAsync(domainEvent);
                    await _outboxRepository.MarkAsProcessedAsync(outboxEvent.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ Error procesando evento {outboxEvent.Id}: {ex.Message}");
                
                await _outboxRepository.IncrementRetryCountAsync(outboxEvent.Id);
                
                if (outboxEvent.RetryCount >= 2) // Máximo 3 intentos
                {
                    await _outboxRepository.MarkAsFailedAsync(outboxEvent.Id, ex.Message);
                }
            }
        }
    }

    private IDomainEvent DeserializeDomainEvent(IOutboxEvent outboxEvent)
    {
        try
        {
            return outboxEvent.EventType switch
            {
                "OrderCreated" => JsonSerializer.Deserialize<OrderCreatedEvent>(outboxEvent.EventData),
                "PaymentProcessed" => JsonSerializer.Deserialize<PaymentProcessedEvent>(outboxEvent.EventData),
                "InventoryReserved" => JsonSerializer.Deserialize<InventoryReservedEvent>(outboxEvent.EventData),
                _ => null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializando evento {outboxEvent.EventType}: {ex.Message}");
            return null;
        }
    }
}

// ============================================================================
// UNIT OF WORK SIMULADO
// ============================================================================

public class SimpleUnitOfWork : IUnitOfWork
{
    private bool _inTransaction = false;

    public Task BeginTransactionAsync()
    {
        _inTransaction = true;
        Console.WriteLine("🔄 Transacción iniciada");
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync()
    {
        if (_inTransaction)
        {
            _inTransaction = false;
            Console.WriteLine("✅ Transacción confirmada");
        }
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync()
    {
        if (_inTransaction)
        {
            _inTransaction = false;
            Console.WriteLine("↩️ Transacción revertida");
        }
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        Console.WriteLine("💾 Cambios guardados en la base de datos");
        return Task.CompletedTask;
    }
}

// ============================================================================
// SERVICIO DE APLICACIÓN
// ============================================================================

public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly OutboxService _outboxService;

    public OrderService(IUnitOfWork unitOfWork, OutboxService outboxService)
    {
        _unitOfWork = unitOfWork;
        _outboxService = outboxService;
    }

    public async Task<Guid> CreateOrderAsync(Guid customerId, string customerEmail, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            OrderDate = DateTime.UtcNow
        };

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // 1. Crear la orden (esto actualiza el estado del agregado)
            order.CreateOrder(customerId, customerEmail, items);

            // 2. Guardar en la base de datos
            await _unitOfWork.SaveChangesAsync();

            // 3. Guardar eventos de dominio en el Outbox (parte de la misma transacción)
            foreach (var domainEvent in order.DomainEvents)
            {
                await _outboxService.SaveDomainEventAsync(domainEvent);
            }

            // 4. Confirmar transacción (garantiza consistencia entre datos y outbox)
            await _unitOfWork.CommitTransactionAsync();

            // 5. Limpiar eventos de dominio del agregado
            order.ClearDomainEvents();

            Console.WriteLine($"🎉 Orden creada exitosamente: {order.Id}");
            return order.Id;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            Console.WriteLine($"❌ Error creando orden: {ex.Message}");
            throw;
        }
    }
}

// ============================================================================
// PROCESADOR EN SEGUNDO PLANO
// ============================================================================

public class OutboxBackgroundProcessor
{
    private readonly OutboxService _outboxService;
    private readonly TimeSpan _processingInterval;
    private bool _isRunning;

    public OutboxBackgroundProcessor(OutboxService outboxService, TimeSpan? processingInterval = null)
    {
        _outboxService = outboxService;
        _processingInterval = processingInterval ?? TimeSpan.FromSeconds(5);
    }

    public async Task StartAsync()
    {
        _isRunning = true;
        Console.WriteLine($"🚀 Procesador de Outbox iniciado (intervalo: {_processingInterval.TotalSeconds}s)");

        while (_isRunning)
        {
            try
            {
                await _outboxService.ProcessPendingEventsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error en procesador de Outbox: {ex.Message}");
            }

            await Task.Delay(_processingInterval);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        Console.WriteLine("🛑 Procesador de Outbox detenido");
    }
}

// ============================================================================
// DEMO REALISTA
// ============================================================================

public static class OutboxDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🏪 DEMO: PATRÓN OUTBOX EN SISTEMA DE E-COMMERCE");
        Console.WriteLine("==============================================");
        Console.WriteLine("Garantiza la entrega de eventos mediante el patrón Outbox\n");

        // Configurar dependencias
        var outboxRepository = new InMemoryOutboxRepository();
        var eventPublisher = new MessageBusEventPublisher();
        var outboxService = new OutboxService(outboxRepository, eventPublisher);
        var unitOfWork = new SimpleUnitOfWork();
        var orderService = new OrderService(unitOfWork, outboxService);

        // Iniciar procesador en segundo plano
        var backgroundProcessor = new OutboxBackgroundProcessor(outboxService, TimeSpan.FromSeconds(3));
        var processorTask = Task.Run(() => backgroundProcessor.StartAsync());

        Console.WriteLine("📋 CREANDO ÓRDENES...");
        Console.WriteLine("====================");

        // Crear varias órdenes para demostrar el patrón
        var orders = new List<Guid>();

        for (int i = 1; i <= 3; i++)
        {
            var customerId = Guid.NewGuid();
            var items = new List<OrderItem>
            {
                new OrderItem { ProductId = Guid.NewGuid(), ProductName = $"Producto {i}A", Quantity = 2, UnitPrice = 29.99m },
                new OrderItem { ProductId = Guid.NewGuid(), ProductName = $"Producto {i}B", Quantity = 1, UnitPrice = 49.99m }
            };

            try
            {
                var orderId = await orderService.CreateOrderAsync(customerId, $"customer{i}@example.com", items);
                orders.Add(orderId);
                
                Console.WriteLine();
                await Task.Delay(1000); // Pausa para ver el flujo
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando orden {i}: {ex.Message}\n");
            }
        }

        Console.WriteLine("\n⏰ ESPERANDO PROCESAMIENTO DE EVENTOS...");
        Console.WriteLine("=======================================");
        
        // Esperar a que se procesen todos los eventos
        await Task.Delay(10000);

        // Estadísticas finales
        Console.WriteLine("\n📊 ESTADÍSTICAS FINALES");
        Console.WriteLine("======================");
        Console.WriteLine($"Total de eventos: {outboxRepository.GetTotalEvents()}");
        Console.WriteLine($"Eventos procesados: {outboxRepository.GetProcessedEvents()}");
        Console.WriteLine($"Órdenes creadas: {orders.Count}");

        // Detener procesador
        backgroundProcessor.Stop();
        
        Console.WriteLine("\n✅ Demo completado");
        Console.WriteLine("\n💡 BENEFICIOS DEL PATRÓN OUTBOX:");
        Console.WriteLine("  • Garantiza que los eventos se publiquen");
        Console.WriteLine("  • Mantiene consistencia entre datos y eventos");
        Console.WriteLine("  • Permite retry automático en caso de fallos");
        Console.WriteLine("  • Procesa eventos de forma asíncrona");
        Console.WriteLine("  • Previene pérdida de eventos críticos");
    }
}
