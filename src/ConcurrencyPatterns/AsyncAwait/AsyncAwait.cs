using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

// ============================================================================
// ASYNC/AWAIT PATTERN - IMPLEMENTACIÓN DIDÁCTICA
// Ejemplo: Sistema de procesamiento de pedidos con operaciones asíncronas
// ============================================================================

// ============================================================================
// MODELOS DE DATOS
// ============================================================================
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class OrderItem
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public enum OrderStatus
{
    Created,
    PaymentProcessing,
    PaymentConfirmed,
    InventoryChecked,
    Shipped,
    Delivered,
    Failed
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; }
    public string Message { get; set; }
}

public class InventoryResult
{
    public bool Available { get; set; }
    public string Message { get; set; }
    public Dictionary<string, int> StockLevels { get; set; } = new Dictionary<string, int>();
}

public class ShippingResult
{
    public bool Success { get; set; }
    public string TrackingNumber { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}

// ============================================================================
// SERVICIOS ASÍNCRONOS - SIMULAN OPERACIONES I/O
// ============================================================================

// Servicio de pagos (simula llamadas a APIs externas)
public class PaymentService
{
    private readonly Random _random = new Random();

    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string customerName)
    {
        Console.WriteLine($"💳 Iniciando procesamiento de pago para {customerName} - ${amount:F2}");
        Console.WriteLine($"   🔄 Conectando con el proveedor de pagos...");
        
        // Simular latencia de red y procesamiento
        await Task.Delay(2000);
        
        // Simular ocasionales fallos de pago (10% probabilidad)
        var success = _random.NextDouble() > 0.1;
        
        if (success)
        {
            var transactionId = $"TXN_{Guid.NewGuid().ToString()[..8].ToUpper()}";
            Console.WriteLine($"   ✅ Pago procesado exitosamente. ID: {transactionId}");
            
            return new PaymentResult
            {
                Success = true,
                TransactionId = transactionId,
                Message = "Payment processed successfully"
            };
        }
        else
        {
            Console.WriteLine($"   ❌ Error en el procesamiento del pago");
            return new PaymentResult
            {
                Success = false,
                Message = "Payment failed - insufficient funds or card declined"
            };
        }
    }

    public async Task<bool> RefundPaymentAsync(string transactionId)
    {
        Console.WriteLine($"🔄 Procesando reembolso para transacción {transactionId}...");
        await Task.Delay(1500); // Simular tiempo de procesamiento
        
        Console.WriteLine($"✅ Reembolso procesado exitosamente");
        return true;
    }
}

// Servicio de inventario (simula consultas a base de datos)
public class InventoryService
{
    private readonly Dictionary<string, int> _inventory;
    private readonly Random _random = new Random();

    public InventoryService()
    {
        // Simular inventario inicial
        _inventory = new Dictionary<string, int>
        {
            { "Laptop", 50 },
            { "Mouse", 200 },
            { "Keyboard", 150 },
            { "Monitor", 75 },
            { "Headphones", 100 }
        };
    }

    public async Task<InventoryResult> CheckInventoryAsync(List<OrderItem> items)
    {
        Console.WriteLine($"📦 Verificando disponibilidad de inventario...");
        
        // Simular tiempo de consulta a base de datos
        await Task.Delay(1000);

        var result = new InventoryResult { Available = true };
        
        foreach (var item in items)
        {
            Console.WriteLine($"   🔍 Verificando {item.ProductName} (cantidad: {item.Quantity})");
            
            if (_inventory.TryGetValue(item.ProductName, out var stock))
            {
                result.StockLevels[item.ProductName] = stock;
                
                if (stock < item.Quantity)
                {
                    result.Available = false;
                    result.Message = $"Insufficient stock for {item.ProductName}. Available: {stock}, Required: {item.Quantity}";
                    Console.WriteLine($"   ❌ Stock insuficiente para {item.ProductName}");
                    break;
                }
                else
                {
                    Console.WriteLine($"   ✅ {item.ProductName} disponible (stock: {stock})");
                }
            }
            else
            {
                result.Available = false;
                result.Message = $"Product {item.ProductName} not found in inventory";
                Console.WriteLine($"   ❌ Producto {item.ProductName} no encontrado");
                break;
            }
        }

        if (result.Available)
        {
            // Reservar inventario
            foreach (var item in items)
            {
                _inventory[item.ProductName] -= item.Quantity;
            }
            Console.WriteLine($"   ✅ Inventario reservado exitosamente");
        }

        return result;
    }

    public async Task ReleaseInventoryAsync(List<OrderItem> items)
    {
        Console.WriteLine($"🔄 Liberando inventario reservado...");
        await Task.Delay(500);
        
        foreach (var item in items)
        {
            if (_inventory.ContainsKey(item.ProductName))
            {
                _inventory[item.ProductName] += item.Quantity;
                Console.WriteLine($"   ↩️ {item.ProductName}: +{item.Quantity} unidades liberadas");
            }
        }
    }
}

// Servicio de envíos (simula integración con couriers)
public class ShippingService
{
    private readonly Random _random = new Random();

    public async Task<ShippingResult> CreateShipmentAsync(Order order)
    {
        Console.WriteLine($"🚚 Creando envío para la orden {order.Id}...");
        Console.WriteLine($"   📍 Destino: {order.CustomerName}");
        
        // Simular tiempo de procesamiento con courier
        await Task.Delay(1500);
        
        var trackingNumber = $"TRK_{_random.Next(100000, 999999)}";
        var estimatedDelivery = DateTime.Now.AddDays(_random.Next(1, 5));
        
        Console.WriteLine($"   ✅ Envío creado. Tracking: {trackingNumber}");
        Console.WriteLine($"   📅 Entrega estimada: {estimatedDelivery:yyyy-MM-dd}");
        
        return new ShippingResult
        {
            Success = true,
            TrackingNumber = trackingNumber,
            EstimatedDelivery = estimatedDelivery
        };
    }
}

// Servicio de notificaciones (simula envío de emails/SMS)
public class NotificationService
{
    public async Task SendOrderConfirmationAsync(Order order)
    {
        Console.WriteLine($"📧 Enviando confirmación de orden a {order.CustomerName}...");
        await Task.Delay(300); // Simular envío de email
        Console.WriteLine($"   ✅ Confirmación enviada");
    }

    public async Task SendShippingNotificationAsync(Order order, string trackingNumber)
    {
        Console.WriteLine($"📧 Enviando notificación de envío a {order.CustomerName}...");
        await Task.Delay(300);
        Console.WriteLine($"   ✅ Notificación de envío enviada (Tracking: {trackingNumber})");
    }

    public async Task SendErrorNotificationAsync(Order order, string error)
    {
        Console.WriteLine($"📧 Enviando notificación de error a {order.CustomerName}...");
        await Task.Delay(300);
        Console.WriteLine($"   ✅ Notificación de error enviada: {error}");
    }
}

// ============================================================================
// PROCESADOR DE ÓRDENES - DEMUESTRA ASYNC/AWAIT PATTERNS
// ============================================================================
public class OrderProcessor
{
    private readonly PaymentService _paymentService;
    private readonly InventoryService _inventoryService;
    private readonly ShippingService _shippingService;
    private readonly NotificationService _notificationService;

    public OrderProcessor()
    {
        _paymentService = new PaymentService();
        _inventoryService = new InventoryService();
        _shippingService = new ShippingService();
        _notificationService = new NotificationService();
    }

    // Procesamiento secuencial de una orden
    public async Task<Order> ProcessOrderSequentiallyAsync(Order order)
    {
        var stopwatch = Stopwatch.StartNew();
        
        Console.WriteLine($"\n🔄 PROCESAMIENTO SECUENCIAL - Orden {order.Id}");
        Console.WriteLine("==============================================");

        try
        {
            order.Status = OrderStatus.PaymentProcessing;

            // Paso 1: Procesar pago
            var paymentResult = await _paymentService.ProcessPaymentAsync(order.TotalAmount, order.CustomerName);
            if (!paymentResult.Success)
            {
                order.Status = OrderStatus.Failed;
                await _notificationService.SendErrorNotificationAsync(order, paymentResult.Message);
                return order;
            }

            order.Status = OrderStatus.PaymentConfirmed;

            // Paso 2: Verificar inventario
            var inventoryResult = await _inventoryService.CheckInventoryAsync(order.Items);
            if (!inventoryResult.Available)
            {
                order.Status = OrderStatus.Failed;
                await _paymentService.RefundPaymentAsync(paymentResult.TransactionId);
                await _notificationService.SendErrorNotificationAsync(order, inventoryResult.Message);
                return order;
            }

            order.Status = OrderStatus.InventoryChecked;

            // Paso 3: Crear envío
            var shippingResult = await _shippingService.CreateShipmentAsync(order);
            order.Status = OrderStatus.Shipped;

            // Paso 4: Enviar notificaciones
            await _notificationService.SendOrderConfirmationAsync(order);
            await _notificationService.SendShippingNotificationAsync(order, shippingResult.TrackingNumber);

            order.ProcessedAt = DateTime.Now;
            stopwatch.Stop();

            Console.WriteLine($"✅ Orden procesada exitosamente en {stopwatch.ElapsedMilliseconds}ms");
            return order;
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            Console.WriteLine($"❌ Error procesando orden: {ex.Message}");
            await _notificationService.SendErrorNotificationAsync(order, ex.Message);
            return order;
        }
    }

    // Procesamiento paralelo optimizado
    public async Task<Order> ProcessOrderParallelAsync(Order order)
    {
        var stopwatch = Stopwatch.StartNew();
        
        Console.WriteLine($"\n⚡ PROCESAMIENTO PARALELO - Orden {order.Id}");
        Console.WriteLine("============================================");

        try
        {
            order.Status = OrderStatus.PaymentProcessing;

            // Ejecutar pago y verificación de inventario en paralelo
            var paymentTask = _paymentService.ProcessPaymentAsync(order.TotalAmount, order.CustomerName);
            var inventoryTask = _inventoryService.CheckInventoryAsync(order.Items);

            // Esperar a que ambas operaciones terminen
            await Task.WhenAll(paymentTask, inventoryTask);

            var paymentResult = paymentTask.Result;
            var inventoryResult = inventoryTask.Result;

            // Verificar resultados
            if (!paymentResult.Success)
            {
                order.Status = OrderStatus.Failed;
                if (inventoryResult.Available)
                {
                    // Liberar inventario si fue reservado pero el pago falló
                    await _inventoryService.ReleaseInventoryAsync(order.Items);
                }
                await _notificationService.SendErrorNotificationAsync(order, paymentResult.Message);
                return order;
            }

            if (!inventoryResult.Available)
            {
                order.Status = OrderStatus.Failed;
                await _paymentService.RefundPaymentAsync(paymentResult.TransactionId);
                await _notificationService.SendErrorNotificationAsync(order, inventoryResult.Message);
                return order;
            }

            order.Status = OrderStatus.InventoryChecked;

            // Crear envío y enviar notificaciones en paralelo
            var shippingTask = _shippingService.CreateShipmentAsync(order);
            var confirmationTask = _notificationService.SendOrderConfirmationAsync(order);

            await Task.WhenAll(shippingTask, confirmationTask);

            var shippingResult = shippingTask.Result;
            order.Status = OrderStatus.Shipped;

            // Enviar notificación de envío
            await _notificationService.SendShippingNotificationAsync(order, shippingResult.TrackingNumber);

            order.ProcessedAt = DateTime.Now;
            stopwatch.Stop();

            Console.WriteLine($"✅ Orden procesada exitosamente en {stopwatch.ElapsedMilliseconds}ms (optimizado)");
            return order;
        }
        catch (Exception ex)
        {
            order.Status = OrderStatus.Failed;
            Console.WriteLine($"❌ Error procesando orden: {ex.Message}");
            await _notificationService.SendErrorNotificationAsync(order, ex.Message);
            return order;
        }
    }

    // Procesamiento de múltiples órdenes con control de concurrencia
    public async Task ProcessMultipleOrdersAsync(List<Order> orders, int maxConcurrency = 3)
    {
        Console.WriteLine($"\n🚀 PROCESAMIENTO DE MÚLTIPLES ÓRDENES");
        Console.WriteLine($"=====================================");
        Console.WriteLine($"Total de órdenes: {orders.Count}");
        Console.WriteLine($"Concurrencia máxima: {maxConcurrency}");

        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = orders.Select(async order =>
        {
            await semaphore.WaitAsync(); // Limitar concurrencia
            try
            {
                return await ProcessOrderParallelAsync(order);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        var successful = results.Count(o => o.Status == OrderStatus.Shipped);
        var failed = results.Count(o => o.Status == OrderStatus.Failed);

        Console.WriteLine($"\n📊 RESUMEN DE PROCESAMIENTO:");
        Console.WriteLine($"   ✅ Exitosas: {successful}");
        Console.WriteLine($"   ❌ Fallidas: {failed}");
        Console.WriteLine($"   ⏱️ Tiempo total: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"   ⚡ Promedio por orden: {stopwatch.ElapsedMilliseconds / orders.Count}ms");
    }
}

// ============================================================================
// DEMO DIDÁCTICO
// ============================================================================
public static class AsyncAwaitPatternDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("⚡ DEMO: ASYNC/AWAIT PATTERN - PROCESAMIENTO DE ÓRDENES");
        Console.WriteLine("========================================================");

        var processor = new OrderProcessor();

        // Crear órdenes de ejemplo
        var orders = new List<Order>
        {
            new Order
            {
                Id = 1001,
                CustomerName = "Juan Pérez",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Laptop", Quantity = 1, Price = 999.99m },
                    new OrderItem { ProductName = "Mouse", Quantity = 2, Price = 29.99m }
                },
                TotalAmount = 1059.97m,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            },
            new Order
            {
                Id = 1002,
                CustomerName = "María García",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Monitor", Quantity = 2, Price = 299.99m },
                    new OrderItem { ProductName = "Keyboard", Quantity = 1, Price = 79.99m }
                },
                TotalAmount = 679.97m,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            },
            new Order
            {
                Id = 1003,
                CustomerName = "Carlos López",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Headphones", Quantity = 3, Price = 149.99m }
                },
                TotalAmount = 449.97m,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            }
        };

        // Demo 1: Procesamiento secuencial vs paralelo
        Console.WriteLine("\n🔍 COMPARACIÓN: SECUENCIAL vs PARALELO");
        Console.WriteLine("======================================");

        var order1 = orders[0];
        await processor.ProcessOrderSequentiallyAsync(order1);

        var order2 = orders[1];
        await processor.ProcessOrderParallelAsync(order2);

        // Demo 2: Procesamiento de múltiples órdenes
        var multipleOrders = orders.Skip(2).ToList();
        // Agregar más órdenes para la demostración
        for (int i = 4; i <= 8; i++)
        {
            multipleOrders.Add(new Order
            {
                Id = 1000 + i,
                CustomerName = $"Cliente {i}",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Mouse", Quantity = 1, Price = 29.99m }
                },
                TotalAmount = 29.99m,
                Status = OrderStatus.Created,
                CreatedAt = DateTime.Now
            });
        }

        await processor.ProcessMultipleOrdersAsync(multipleOrders, maxConcurrency: 3);

        // Demo 3: Manejo de timeouts
        Console.WriteLine("\n⏰ DEMO: MANEJO DE TIMEOUTS");
        Console.WriteLine("===========================");
        await DemoTimeoutHandling();

        // Demo 4: Cancelación de operaciones
        Console.WriteLine("\n🛑 DEMO: CANCELACIÓN DE OPERACIONES");
        Console.WriteLine("==================================");
        await DemoCancellation();

        Console.WriteLine("\n✅ Demo del Async/Await Pattern completado");
        Console.WriteLine("\n💡 LECCIONES APRENDIDAS:");
        Console.WriteLine("   • async/await permite código no bloqueante y legible");
        Console.WriteLine("   • Task.WhenAll ejecuta operaciones en paralelo");
        Console.WriteLine("   • SemaphoreSlim controla la concurrencia");
        Console.WriteLine("   • Importante manejar excepciones y timeouts");
        Console.WriteLine("   • El procesamiento paralelo puede mejorar significativamente el rendimiento");
    }

    private static async Task DemoTimeoutHandling()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            
            Console.WriteLine("🔄 Simulando operación con timeout de 2 segundos...");
            
            // Simular operación lenta
            await Task.Delay(5000, cts.Token);
            
            Console.WriteLine("✅ Operación completada");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("⏰ Operación cancelada por timeout");
        }
    }

    private static async Task DemoCancellation()
    {
        using var cts = new CancellationTokenSource();
        
        // Cancelar después de 1 segundo
        cts.CancelAfter(1000);
        
        try
        {
            Console.WriteLine("🔄 Iniciando operación cancelable...");
            
            for (int i = 0; i < 10; i++)
            {
                cts.Token.ThrowIfCancellationRequested();
                Console.WriteLine($"   Paso {i + 1}/10");
                await Task.Delay(500, cts.Token);
            }
            
            Console.WriteLine("✅ Operación completada sin cancelación");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("🛑 Operación cancelada por solicitud del usuario");
        }
    }
}

// Para ejecutar el demo:
// await AsyncAwaitPatternDemo.RunDemo();
