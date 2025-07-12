using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

// ============================================================================
// ORCHESTRATOR PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de e-commerce con orquestación de flujo complejo de orden
// ============================================================================

// ============================================================================
// WORKFLOW PRIMITIVES - ELEMENTOS BÁSICOS DEL FLUJO
// ============================================================================

/// <summary>
/// Resultado de un paso del workflow
/// </summary>
public class StepResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public object Data { get; }
    public Exception Exception { get; }

    private StepResult(bool isSuccess, string message, object data = null, Exception exception = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
        Exception = exception;
    }

    public static StepResult Success(string message, object data = null) => new(true, message, data);
    public static StepResult Failure(string message, Exception exception = null) => new(false, message, exception: exception);
}

/// <summary>
/// Contexto compartido entre pasos del workflow
/// </summary>
public class WorkflowContext
{
    private readonly Dictionary<string, object> _data = new();

    public T Get<T>(string key)
    {
        return _data.ContainsKey(key) ? (T)_data[key] : default(T);
    }

    public void Set<T>(string key, T value)
    {
        _data[key] = value;
    }

    public bool Contains(string key) => _data.ContainsKey(key);

    public void Remove(string key)
    {
        _data.Remove(key);
    }

    public Dictionary<string, object> GetAllData() => new(_data);
}

/// <summary>
/// Interfaz para pasos del workflow
/// </summary>
public interface IWorkflowStep
{
    string StepName { get; }
    Task<StepResult> ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
    Task<StepResult> CompensateAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Estado del workflow
/// </summary>
public enum WorkflowStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed,
    Compensating,
    Compensated
}

// ============================================================================
// WORKFLOW ENGINE - MOTOR DE ORQUESTACIÓN
// ============================================================================

/// <summary>
/// Orquestador principal que maneja el flujo de trabajo
/// </summary>
public class WorkflowOrchestrator
{
    private readonly List<IWorkflowStep> _steps = new();
    private readonly List<string> _executedSteps = new();
    private readonly WorkflowContext _context = new();

    public string WorkflowId { get; } = Guid.NewGuid().ToString();
    public WorkflowStatus Status { get; private set; } = WorkflowStatus.NotStarted;
    public string CurrentStep { get; private set; }
    public List<string> ExecutedSteps => new(_executedSteps);
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string LastError { get; private set; }

    // Eventos para monitoreo
    public event EventHandler<WorkflowStepCompletedEventArgs> StepCompleted;
    public event EventHandler<WorkflowStepFailedEventArgs> StepFailed;
    public event EventHandler<WorkflowCompletedEventArgs> WorkflowCompleted;
    public event EventHandler<WorkflowFailedEventArgs> WorkflowFailed;

    public WorkflowOrchestrator AddStep(IWorkflowStep step)
    {
        _steps.Add(step);
        return this;
    }

    public WorkflowOrchestrator SetInitialData<T>(string key, T value)
    {
        _context.Set(key, value);
        return this;
    }

    public async Task<WorkflowResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        StartedAt = DateTime.UtcNow;
        Status = WorkflowStatus.InProgress;

        Console.WriteLine($"🚀 Iniciando workflow {WorkflowId}");
        Console.WriteLine($"   Pasos configurados: {_steps.Count}");
        Console.WriteLine($"   Datos iniciales: {_context.GetAllData().Count} elementos");

        try
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                var step = _steps[i];
                CurrentStep = step.StepName;

                Console.WriteLine($"\n⚡ Ejecutando paso {i + 1}/{_steps.Count}: {step.StepName}");

                var stepResult = await step.ExecuteAsync(_context, cancellationToken);

                if (stepResult.IsSuccess)
                {
                    _executedSteps.Add(step.StepName);
                    Console.WriteLine($"   ✅ {step.StepName} completado: {stepResult.Message}");
                    
                    StepCompleted?.Invoke(this, new WorkflowStepCompletedEventArgs
                    {
                        WorkflowId = WorkflowId,
                        StepName = step.StepName,
                        Message = stepResult.Message,
                        Data = stepResult.Data
                    });
                }
                else
                {
                    Console.WriteLine($"   ❌ {step.StepName} falló: {stepResult.Message}");
                    LastError = stepResult.Message;
                    
                    StepFailed?.Invoke(this, new WorkflowStepFailedEventArgs
                    {
                        WorkflowId = WorkflowId,
                        StepName = step.StepName,
                        Error = stepResult.Message,
                        Exception = stepResult.Exception
                    });

                    // Iniciar compensación
                    await CompensateAsync(cancellationToken);
                    return new WorkflowResult(false, $"Workflow falló en paso '{step.StepName}': {stepResult.Message}");
                }
            }

            // Workflow completado exitosamente
            Status = WorkflowStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            CurrentStep = null;

            Console.WriteLine($"🎉 Workflow {WorkflowId} completado exitosamente");
            Console.WriteLine($"   Duración: {(CompletedAt.Value - StartedAt).TotalSeconds:F2} segundos");

            WorkflowCompleted?.Invoke(this, new WorkflowCompletedEventArgs
            {
                WorkflowId = WorkflowId,
                Duration = CompletedAt.Value - StartedAt,
                ExecutedSteps = new List<string>(_executedSteps)
            });

            return new WorkflowResult(true, "Workflow completado exitosamente", _context.GetAllData());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Error inesperado en workflow: {ex.Message}");
            LastError = ex.Message;
            
            WorkflowFailed?.Invoke(this, new WorkflowFailedEventArgs
            {
                WorkflowId = WorkflowId,
                Error = ex.Message,
                Exception = ex
            });

            await CompensateAsync(cancellationToken);
            return new WorkflowResult(false, $"Error inesperado: {ex.Message}");
        }
    }

    private async Task CompensateAsync(CancellationToken cancellationToken = default)
    {
        if (!_executedSteps.Any())
        {
            Console.WriteLine("📝 No hay pasos para compensar");
            Status = WorkflowStatus.Failed;
            return;
        }

        Status = WorkflowStatus.Compensating;
        Console.WriteLine($"\n🔄 Iniciando compensación de {_executedSteps.Count} pasos ejecutados");

        // Compensar en orden reverso
        for (int i = _executedSteps.Count - 1; i >= 0; i--)
        {
            var executedStepName = _executedSteps[i];
            var step = _steps.First(s => s.StepName == executedStepName);

            Console.WriteLine($"   🔙 Compensando: {step.StepName}");

            try
            {
                var compensationResult = await step.CompensateAsync(_context, cancellationToken);
                
                if (compensationResult.IsSuccess)
                {
                    Console.WriteLine($"   ✅ Compensación exitosa: {compensationResult.Message}");
                }
                else
                {
                    Console.WriteLine($"   ⚠️ Compensación falló: {compensationResult.Message}");
                    // Continuar con otras compensaciones aunque esta falle
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   💥 Error en compensación: {ex.Message}");
            }
        }

        Status = WorkflowStatus.Compensated;
        CompletedAt = DateTime.UtcNow;
        Console.WriteLine("🔚 Compensación completada");
    }

    public T GetData<T>(string key) => _context.Get<T>(key);
}

/// <summary>
/// Resultado del workflow
/// </summary>
public class WorkflowResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public Dictionary<string, object> Data { get; }

    public WorkflowResult(bool isSuccess, string message, Dictionary<string, object> data = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data ?? new Dictionary<string, object>();
    }
}

// ============================================================================
// EVENT ARGS PARA MONITOREO
// ============================================================================

public class WorkflowStepCompletedEventArgs : EventArgs
{
    public string WorkflowId { get; set; }
    public string StepName { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
}

public class WorkflowStepFailedEventArgs : EventArgs
{
    public string WorkflowId { get; set; }
    public string StepName { get; set; }
    public string Error { get; set; }
    public Exception Exception { get; set; }
}

public class WorkflowCompletedEventArgs : EventArgs
{
    public string WorkflowId { get; set; }
    public TimeSpan Duration { get; set; }
    public List<string> ExecutedSteps { get; set; }
}

public class WorkflowFailedEventArgs : EventArgs
{
    public string WorkflowId { get; set; }
    public string Error { get; set; }
    public Exception Exception { get; set; }
}

// ============================================================================
// IMPLEMENTACIÓN DE PASOS PARA E-COMMERCE
// ============================================================================

/// <summary>
/// Datos de la orden para el workflow
/// </summary>
public class OrderData
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemData> Items { get; set; } = new();
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
}

public class OrderItemData
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Paso 1: Validar orden
/// </summary>
public class ValidateOrderStep : IWorkflowStep
{
    public string StepName => "ValidateOrder";

    public async Task<StepResult> ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(500, cancellationToken); // Simular procesamiento

        var orderData = context.Get<OrderData>("OrderData");
        
        if (orderData == null)
            return StepResult.Failure("No se encontraron datos de la orden");

        if (orderData.TotalAmount <= 0)
            return StepResult.Failure("El monto de la orden debe ser mayor a cero");

        if (!orderData.Items.Any())
            return StepResult.Failure("La orden debe tener al menos un item");

        if (string.IsNullOrWhiteSpace(orderData.CustomerEmail))
            return StepResult.Failure("Email del cliente es requerido");

        context.Set("ValidationTimestamp", DateTime.UtcNow);
        return StepResult.Success("Orden validada correctamente");
    }

    public async Task<StepResult> CompensateAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        context.Remove("ValidationTimestamp");
        return StepResult.Success("Validación revertida");
    }
}

/// <summary>
/// Paso 2: Reservar inventario
/// </summary>
public class ReserveInventoryStep : IWorkflowStep
{
    private readonly Dictionary<Guid, int> _inventory = new()
    {
        { Guid.Parse("11111111-1111-1111-1111-111111111111"), 50 },
        { Guid.Parse("22222222-2222-2222-2222-222222222222"), 100 },
        { Guid.Parse("33333333-3333-3333-3333-333333333333"), 25 }
    };

    public string StepName => "ReserveInventory";

    public async Task<StepResult> ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(800, cancellationToken);

        var orderData = context.Get<OrderData>("OrderData");
        var reservedItems = new List<(Guid ProductId, int Quantity)>();

        foreach (var item in orderData.Items)
        {
            if (!_inventory.ContainsKey(item.ProductId))
                return StepResult.Failure($"Producto {item.ProductName} no encontrado en inventario");

            if (_inventory[item.ProductId] < item.Quantity)
                return StepResult.Failure($"Stock insuficiente para {item.ProductName}. Disponible: {_inventory[item.ProductId]}, Solicitado: {item.Quantity}");

            _inventory[item.ProductId] -= item.Quantity;
            reservedItems.Add((item.ProductId, item.Quantity));
        }

        var reservationId = Guid.NewGuid();
        context.Set("ReservationId", reservationId);
        context.Set("ReservedItems", reservedItems);

        return StepResult.Success($"Inventario reservado. ID: {reservationId}");
    }

    public async Task<StepResult> CompensateAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken);

        var reservedItems = context.Get<List<(Guid ProductId, int Quantity)>>("ReservedItems");
        
        if (reservedItems != null)
        {
            foreach (var (productId, quantity) in reservedItems)
            {
                _inventory[productId] += quantity;
            }
            
            context.Remove("ReservationId");
            context.Remove("ReservedItems");
        }

        return StepResult.Success("Reserva de inventario revertida");
    }
}

/// <summary>
/// Paso 3: Procesar pago
/// </summary>
public class ProcessPaymentStep : IWorkflowStep
{
    public string StepName => "ProcessPayment";

    public async Task<StepResult> ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1200, cancellationToken);

        var orderData = context.Get<OrderData>("OrderData");

        // Simular procesamiento de pago (80% de éxito)
        var isSuccess = new Random().Next(1, 10) <= 8;

        if (!isSuccess)
            return StepResult.Failure("Pago rechazado por el banco");

        var transactionId = $"TXN-{Guid.NewGuid().ToString()[..8]}";
        context.Set("TransactionId", transactionId);
        context.Set("PaymentAmount", orderData.TotalAmount);
        context.Set("PaymentTimestamp", DateTime.UtcNow);

        return StepResult.Success($"Pago procesado exitosamente. Transaction ID: {transactionId}");
    }

    public async Task<StepResult> CompensateAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(800, cancellationToken);

        var transactionId = context.Get<string>("TransactionId");
        var amount = context.Get<decimal>("PaymentAmount");

        if (!string.IsNullOrEmpty(transactionId))
        {
            // Simular refund
            var refundId = $"REF-{Guid.NewGuid().ToString()[..8]}";
            context.Set("RefundId", refundId);
            
            context.Remove("TransactionId");
            context.Remove("PaymentAmount");
            context.Remove("PaymentTimestamp");
        }

        return StepResult.Success($"Pago revertido. Refund procesado: {context.Get<string>("RefundId")}");
    }
}

/// <summary>
/// Paso 4: Crear envío
/// </summary>
public class CreateShipmentStep : IWorkflowStep
{
    public string StepName => "CreateShipment";

    public async Task<StepResult> ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(600, cancellationToken);

        var orderData = context.Get<OrderData>("OrderData");
        var trackingNumber = $"TRK-{Guid.NewGuid().ToString()[..8]}";
        var estimatedDelivery = DateTime.UtcNow.AddDays(3);

        context.Set("TrackingNumber", trackingNumber);
        context.Set("EstimatedDelivery", estimatedDelivery);
        context.Set("ShipmentCreatedAt", DateTime.UtcNow);

        return StepResult.Success($"Envío creado. Tracking: {trackingNumber}, Entrega estimada: {estimatedDelivery:yyyy-MM-dd}");
    }

    public async Task<StepResult> CompensateAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(400, cancellationToken);

        var trackingNumber = context.Get<string>("TrackingNumber");
        
        if (!string.IsNullOrEmpty(trackingNumber))
        {
            context.Remove("TrackingNumber");
            context.Remove("EstimatedDelivery");
            context.Remove("ShipmentCreatedAt");
        }

        return StepResult.Success($"Envío cancelado: {trackingNumber}");
    }
}

/// <summary>
/// Paso 5: Enviar notificaciones
/// </summary>
public class SendNotificationStep : IWorkflowStep
{
    public string StepName => "SendNotification";

    public async Task<StepResult> ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(400, cancellationToken);

        var orderData = context.Get<OrderData>("OrderData");
        var trackingNumber = context.Get<string>("TrackingNumber");
        var transactionId = context.Get<string>("TransactionId");

        var notifications = new List<string>
        {
            $"Email confirmación a {orderData.CustomerEmail}",
            $"SMS con tracking {trackingNumber}",
            $"Notificación push de pago {transactionId}"
        };

        context.Set("SentNotifications", notifications);
        context.Set("NotificationTimestamp", DateTime.UtcNow);

        return StepResult.Success($"Notificaciones enviadas: {notifications.Count}");
    }

    public async Task<StepResult> CompensateAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        await Task.Delay(200, cancellationToken);

        var notifications = context.Get<List<string>>("SentNotifications");
        
        if (notifications != null)
        {
            // En un caso real, aquí enviarías notificaciones de cancelación
            context.Set("CancellationNotificationsSent", notifications.Count);
            context.Remove("SentNotifications");
            context.Remove("NotificationTimestamp");
        }

        return StepResult.Success("Notificaciones de cancelación enviadas");
    }
}

// ============================================================================
// WORKFLOW FACTORY - FÁBRICA DE WORKFLOWS
// ============================================================================

public static class OrderWorkflowFactory
{
    public static WorkflowOrchestrator CreateOrderProcessingWorkflow(OrderData orderData)
    {
        var workflow = new WorkflowOrchestrator()
            .SetInitialData("OrderData", orderData)
            .AddStep(new ValidateOrderStep())
            .AddStep(new ReserveInventoryStep())
            .AddStep(new ProcessPaymentStep())
            .AddStep(new CreateShipmentStep())
            .AddStep(new SendNotificationStep());

        // Suscribir a eventos para logging
        workflow.StepCompleted += (sender, e) =>
        {
            Console.WriteLine($"📊 LOG: Paso completado - {e.StepName} en workflow {e.WorkflowId}");
        };

        workflow.StepFailed += (sender, e) =>
        {
            Console.WriteLine($"📊 LOG: Paso falló - {e.StepName} en workflow {e.WorkflowId}: {e.Error}");
        };

        workflow.WorkflowCompleted += (sender, e) =>
        {
            Console.WriteLine($"📊 LOG: Workflow {e.WorkflowId} completado en {e.Duration.TotalSeconds:F2}s");
        };

        workflow.WorkflowFailed += (sender, e) =>
        {
            Console.WriteLine($"📊 LOG: Workflow {e.WorkflowId} falló: {e.Error}");
        };

        return workflow;
    }
}

// ============================================================================
// DEMO REALISTA DEL ORCHESTRATOR
// ============================================================================

public static class OrchestratorDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🎯 DEMO: ORCHESTRATOR PATTERN");
        Console.WriteLine("==============================");
        Console.WriteLine("Demostración de orquestación de flujo complejo de e-commerce\n");

        // Crear datos de orden de prueba
        var orders = new[]
        {
            new OrderData
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "cliente1@example.com",
                TotalAmount = 1299.98m,
                ShippingAddress = "Av. Reforma 123, CDMX",
                PaymentMethod = "Visa ****4444",
                Items = new List<OrderItemData>
                {
                    new() { ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ProductName = "Laptop Gaming", Quantity = 1, UnitPrice = 999.99m },
                    new() { ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ProductName = "Mouse Gaming", Quantity = 2, UnitPrice = 149.99m }
                }
            },
            new OrderData
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "cliente2@example.com",
                TotalAmount = 259.98m,
                ShippingAddress = "Calle Principal 456, Guadalajara",
                PaymentMethod = "Mastercard ****1234",
                Items = new List<OrderItemData>
                {
                    new() { ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"), ProductName = "Teclado Mecánico", Quantity = 2, UnitPrice = 129.99m }
                }
            },
            new OrderData
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerEmail = "cliente3@example.com",
                TotalAmount = 49999.99m, // Orden muy costosa para provocar fallo en pago
                ShippingAddress = "Boulevard Sur 789, Monterrey",
                PaymentMethod = "Visa ****9999",
                Items = new List<OrderItemData>
                {
                    new() { ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ProductName = "Laptop Gaming", Quantity = 50, UnitPrice = 999.99m } // Mucho inventario
                }
            }
        };

        var results = new List<(OrderData Order, WorkflowResult Result)>();

        for (int i = 0; i < orders.Length; i++)
        {
            var order = orders[i];
            
            Console.WriteLine($"\n🛒 PROCESANDO ORDEN {i + 1}/{orders.Length}:");
            Console.WriteLine("==========================================");
            Console.WriteLine($"ID: {order.OrderId}");
            Console.WriteLine($"Cliente: {order.CustomerEmail}");
            Console.WriteLine($"Total: ${order.TotalAmount:F2}");
            Console.WriteLine($"Items: {order.Items.Count}");

            var workflow = OrderWorkflowFactory.CreateOrderProcessingWorkflow(order);
            var result = await workflow.ExecuteAsync();

            results.Add((order, result));

            Console.WriteLine($"\n📋 RESULTADO ORDEN {i + 1}:");
            Console.WriteLine($"✅ Éxito: {result.IsSuccess}");
            Console.WriteLine($"📝 Mensaje: {result.Message}");
            
            if (result.IsSuccess)
            {
                Console.WriteLine("🎉 ¡Orden procesada exitosamente!");
                if (workflow.GetData<string>("TrackingNumber") != null)
                {
                    Console.WriteLine($"📦 Tracking: {workflow.GetData<string>("TrackingNumber")}");
                    Console.WriteLine($"🚚 Entrega estimada: {workflow.GetData<DateTime>("EstimatedDelivery"):yyyy-MM-dd}");
                }
            }
            else
            {
                Console.WriteLine("❌ Orden falló - compensación aplicada");
            }

            if (i < orders.Length - 1)
            {
                Console.WriteLine("\n⏳ Esperando antes de la siguiente orden...");
                await Task.Delay(2000);
            }
        }

        // Resumen final
        Console.WriteLine("\n📊 RESUMEN FINAL:");
        Console.WriteLine("==================");
        var successful = results.Count(r => r.Result.IsSuccess);
        var failed = results.Count(r => !r.Result.IsSuccess);

        Console.WriteLine($"Total órdenes procesadas: {results.Count}");
        Console.WriteLine($"✅ Exitosas: {successful}");
        Console.WriteLine($"❌ Fallidas: {failed}");
        Console.WriteLine($"📈 Tasa de éxito: {(successful / (double)results.Count * 100):F1}%");

        Console.WriteLine("\n💡 BENEFICIOS DEL ORCHESTRATOR PATTERN:");
        Console.WriteLine("  • Control centralizado del flujo de trabajo");
        Console.WriteLine("  • Manejo automático de compensaciones");
        Console.WriteLine("  • Visibilidad completa del proceso");
        Console.WriteLine("  • Facilidad para agregar/modificar pasos");
        Console.WriteLine("  • Monitoreo y logging integrado");
        Console.WriteLine("  • Recuperación ante fallos");
    }
}
