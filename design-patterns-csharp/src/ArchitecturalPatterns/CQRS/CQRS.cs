using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ============================================================================
// CQRS PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de e-commerce con separación de comandos y consultas
// ============================================================================

// ============================================================================
// INFRAESTRUCTURA BASE
// ============================================================================

public interface ICommand { }
public interface IQuery<TResult> { }
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command);
}
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}

// Event para notificar cambios
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
    string EventType { get; }
}

// ============================================================================
// MODELOS DE DOMINIO
// ============================================================================

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public string ShippingAddress { get; set; }
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

// ============================================================================
// COMANDOS (WRITE SIDE)
// ============================================================================

public class CreateProductCommand : ICommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int InitialStock { get; set; }
    public string Category { get; set; }
}

public class UpdateProductStockCommand : ICommand
{
    public Guid ProductId { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; }
}

public class CreateOrderCommand : ICommand
{
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    public string ShippingAddress { get; set; }
}

public class UpdateOrderStatusCommand : ICommand
{
    public Guid OrderId { get; set; }
    public OrderStatus NewStatus { get; set; }
    public string Notes { get; set; }
}

public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// ============================================================================
// CONSULTAS (READ SIDE)
// ============================================================================

public class GetProductByIdQuery : IQuery<ProductReadModel>
{
    public Guid ProductId { get; set; }
}

public class GetProductsByCategoryQuery : IQuery<List<ProductSummaryReadModel>>
{
    public string Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetOrdersByCustomerQuery : IQuery<List<OrderSummaryReadModel>>
{
    public Guid CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetOrderDetailsQuery : IQuery<OrderDetailsReadModel>
{
    public Guid OrderId { get; set; }
}

public class GetLowStockProductsQuery : IQuery<List<ProductSummaryReadModel>>
{
    public int ThresholdQuantity { get; set; } = 10;
}

// ============================================================================
// MODELOS DE LECTURA (READ MODELS)
// ============================================================================

public class ProductReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; }
    public bool IsActive { get; set; }
    public string StockStatus => StockQuantity > 10 ? "En stock" : 
                                StockQuantity > 0 ? "Stock bajo" : "Agotado";
}

public class ProductSummaryReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; }
    public string StockStatus { get; set; }
}

public class OrderSummaryReadModel
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public int ItemCount { get; set; }
}

public class OrderDetailsReadModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; }
    public List<OrderItemReadModel> Items { get; set; } = new List<OrderItemReadModel>();
}

public class OrderItemReadModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

// ============================================================================
// EVENTOS DE DOMINIO
// ============================================================================

public class ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "ProductCreated";
}

public class OrderCreatedEvent : IDomainEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderCreated";
}

public class StockUpdatedEvent : IDomainEvent
{
    public Guid ProductId { get; set; }
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "StockUpdated";
}

// ============================================================================
// ALMACENES (WRITE STORE)
// ============================================================================

public class ProductWriteStore
{
    private readonly Dictionary<Guid, Product> _products = new Dictionary<Guid, Product>();

    public async Task<Product> GetByIdAsync(Guid id)
    {
        await Task.Delay(10); // Simular latencia de DB
        return _products.TryGetValue(id, out var product) ? product : null;
    }

    public async Task SaveAsync(Product product)
    {
        await Task.Delay(50); // Simular latencia de escritura
        _products[product.Id] = product;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        await Task.Delay(20);
        return _products.Values.ToList();
    }
}

public class OrderWriteStore
{
    private readonly Dictionary<Guid, Order> _orders = new Dictionary<Guid, Order>();

    public async Task<Order> GetByIdAsync(Guid id)
    {
        await Task.Delay(10);
        return _orders.TryGetValue(id, out var order) ? order : null;
    }

    public async Task SaveAsync(Order order)
    {
        await Task.Delay(50);
        _orders[order.Id] = order;
    }

    public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        await Task.Delay(20);
        return _orders.Values.Where(o => o.CustomerId == customerId).ToList();
    }
}

// ============================================================================
// COMMAND HANDLERS
// ============================================================================

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly ProductWriteStore _productStore;
    private readonly List<IDomainEvent> _events;

    public CreateProductCommandHandler(ProductWriteStore productStore, List<IDomainEvent> events)
    {
        _productStore = productStore;
        _events = events;
    }

    public async Task HandleAsync(CreateProductCommand command)
    {
        Console.WriteLine($"📝 Procesando comando: Crear producto '{command.Name}'");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            StockQuantity = command.InitialStock,
            Category = command.Category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _productStore.SaveAsync(product);

        // Publicar evento
        _events.Add(new ProductCreatedEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Category = product.Category
        });

        Console.WriteLine($"✅ Producto creado: ID {product.Id}");
    }
}

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly OrderWriteStore _orderStore;
    private readonly ProductWriteStore _productStore;
    private readonly List<IDomainEvent> _events;

    public CreateOrderCommandHandler(OrderWriteStore orderStore, ProductWriteStore productStore, List<IDomainEvent> events)
    {
        _orderStore = orderStore;
        _productStore = productStore;
        _events = events;
    }

    public async Task HandleAsync(CreateOrderCommand command)
    {
        Console.WriteLine($"📝 Procesando comando: Crear orden para cliente {command.CustomerEmail}");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = command.CustomerId,
            CustomerEmail = command.CustomerEmail,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            ShippingAddress = command.ShippingAddress
        };

        decimal totalAmount = 0;

        foreach (var item in command.Items)
        {
            var product = await _productStore.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Producto {item.ProductId} no encontrado");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Stock insuficiente para {product.Name}");
            }

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            order.Items.Add(orderItem);
            totalAmount += orderItem.TotalPrice;

            // Actualizar stock
            product.StockQuantity -= item.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _productStore.SaveAsync(product);
        }

        order.TotalAmount = totalAmount;
        await _orderStore.SaveAsync(order);

        // Publicar evento
        _events.Add(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Count
        });

        Console.WriteLine($"✅ Orden creada: ID {order.Id}, Total: ${order.TotalAmount:F2}");
    }
}

public class UpdateProductStockCommandHandler : ICommandHandler<UpdateProductStockCommand>
{
    private readonly ProductWriteStore _productStore;
    private readonly List<IDomainEvent> _events;

    public UpdateProductStockCommandHandler(ProductWriteStore productStore, List<IDomainEvent> events)
    {
        _productStore = productStore;
        _events = events;
    }

    public async Task HandleAsync(UpdateProductStockCommand command)
    {
        Console.WriteLine($"📝 Procesando comando: Actualizar stock del producto {command.ProductId}");

        var product = await _productStore.GetByIdAsync(command.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Producto {command.ProductId} no encontrado");
        }

        var oldQuantity = product.StockQuantity;
        product.StockQuantity = command.NewQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _productStore.SaveAsync(product);

        // Publicar evento
        _events.Add(new StockUpdatedEvent
        {
            ProductId = product.Id,
            OldQuantity = oldQuantity,
            NewQuantity = command.NewQuantity,
            Reason = command.Reason
        });

        Console.WriteLine($"✅ Stock actualizado: {oldQuantity} → {command.NewQuantity} ({command.Reason})");
    }
}

// ============================================================================
// QUERY HANDLERS (READ SIDE)
// ============================================================================

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductReadModel>
{
    private readonly ProductWriteStore _productStore; // En un escenario real, sería un Read Store optimizado

    public GetProductByIdQueryHandler(ProductWriteStore productStore)
    {
        _productStore = productStore;
    }

    public async Task<ProductReadModel> HandleAsync(GetProductByIdQuery query)
    {
        Console.WriteLine($"🔍 Procesando consulta: Obtener producto {query.ProductId}");

        var product = await _productStore.GetByIdAsync(query.ProductId);
        if (product == null) return null;

        return new ProductReadModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            IsActive = product.IsActive
        };
    }
}

public class GetProductsByCategoryQueryHandler : IQueryHandler<GetProductsByCategoryQuery, List<ProductSummaryReadModel>>
{
    private readonly ProductWriteStore _productStore;

    public GetProductsByCategoryQueryHandler(ProductWriteStore productStore)
    {
        _productStore = productStore;
    }

    public async Task<List<ProductSummaryReadModel>> HandleAsync(GetProductsByCategoryQuery query)
    {
        Console.WriteLine($"🔍 Procesando consulta: Obtener productos de categoría '{query.Category}'");

        var allProducts = await _productStore.GetAllAsync();
        var categoryProducts = allProducts
            .Where(p => p.Category.Equals(query.Category, StringComparison.OrdinalIgnoreCase) && p.IsActive)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductSummaryReadModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
                StockStatus = p.StockQuantity > 10 ? "En stock" : 
                             p.StockQuantity > 0 ? "Stock bajo" : "Agotado"
            })
            .ToList();

        return categoryProducts;
    }
}

public class GetOrdersByCustomerQueryHandler : IQueryHandler<GetOrdersByCustomerQuery, List<OrderSummaryReadModel>>
{
    private readonly OrderWriteStore _orderStore;

    public GetOrdersByCustomerQueryHandler(OrderWriteStore orderStore)
    {
        _orderStore = orderStore;
    }

    public async Task<List<OrderSummaryReadModel>> HandleAsync(GetOrdersByCustomerQuery query)
    {
        Console.WriteLine($"🔍 Procesando consulta: Obtener órdenes del cliente {query.CustomerId}");

        var orders = await _orderStore.GetByCustomerIdAsync(query.CustomerId);
        
        var filteredOrders = orders.AsQueryable();
        if (query.FromDate.HasValue)
            filteredOrders = filteredOrders.Where(o => o.OrderDate >= query.FromDate.Value);
        if (query.ToDate.HasValue)
            filteredOrders = filteredOrders.Where(o => o.OrderDate <= query.ToDate.Value);

        return filteredOrders
            .Select(o => new OrderSummaryReadModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ItemCount = o.Items.Count
            })
            .OrderByDescending(o => o.OrderDate)
            .ToList();
    }
}

// ============================================================================
// DEMO REALISTA
// ============================================================================
public static class CqrsDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🏪 DEMO: CQRS EN SISTEMA DE E-COMMERCE");
        Console.WriteLine("======================================");

        // Configurar dependencias
        var productStore = new ProductWriteStore();
        var orderStore = new OrderWriteStore();
        var events = new List<IDomainEvent>();

        // Command handlers
        var createProductHandler = new CreateProductCommandHandler(productStore, events);
        var createOrderHandler = new CreateOrderCommandHandler(orderStore, productStore, events);
        var updateStockHandler = new UpdateProductStockCommandHandler(productStore, events);

        // Query handlers
        var getProductHandler = new GetProductByIdQueryHandler(productStore);
        var getProductsByCategoryHandler = new GetProductsByCategoryQueryHandler(productStore);
        var getOrdersByCustomerHandler = new GetOrdersByCustomerQueryHandler(orderStore);

        Console.WriteLine("\n📦 CREANDO PRODUCTOS...");

        // Crear productos
        await createProductHandler.HandleAsync(new CreateProductCommand
        {
            Name = "MacBook Pro 16\"",
            Description = "Laptop profesional de alta gama",
            Price = 2499.99m,
            InitialStock = 25,
            Category = "Laptops"
        });

        await createProductHandler.HandleAsync(new CreateProductCommand
        {
            Name = "iPhone 15 Pro",
            Description = "Smartphone último modelo",
            Price = 999.99m,
            InitialStock = 50,
            Category = "Smartphones"
        });

        await createProductHandler.HandleAsync(new CreateProductCommand
        {
            Name = "iPad Air",
            Description = "Tablet versátil para trabajo y entretenimiento",
            Price = 599.99m,
            InitialStock = 30,
            Category = "Tablets"
        });

        Console.WriteLine("\n🔍 CONSULTANDO PRODUCTOS POR CATEGORÍA...");

        // Consultar productos
        var laptops = await getProductsByCategoryHandler.HandleAsync(new GetProductsByCategoryQuery
        {
            Category = "Laptops",
            Page = 1,
            PageSize = 10
        });

        foreach (var laptop in laptops)
        {
            Console.WriteLine($"   💻 {laptop.Name} - ${laptop.Price} - {laptop.StockStatus}");
        }

        Console.WriteLine("\n🛒 CREANDO ÓRDENES...");

        var customerId = Guid.NewGuid();
        var macbookId = laptops.First().Id;
        
        // Consultar producto específico para obtener ID del iPhone
        var smartphones = await getProductsByCategoryHandler.HandleAsync(new GetProductsByCategoryQuery { Category = "Smartphones" });
        var iphoneId = smartphones.First().Id;

        // Crear orden
        await createOrderHandler.HandleAsync(new CreateOrderCommand
        {
            CustomerId = customerId,
            CustomerEmail = "juan@ejemplo.com",
            ShippingAddress = "Calle Principal 123, Ciudad",
            Items = new List<OrderItemRequest>
            {
                new OrderItemRequest { ProductId = macbookId, Quantity = 1 },
                new OrderItemRequest { ProductId = iphoneId, Quantity = 2 }
            }
        });

        Console.WriteLine("\n📊 CONSULTANDO ÓRDENES DEL CLIENTE...");

        var customerOrders = await getOrdersByCustomerHandler.HandleAsync(new GetOrdersByCustomerQuery
        {
            CustomerId = customerId
        });

        foreach (var order in customerOrders)
        {
            Console.WriteLine($"   📋 Orden {order.Id.ToString()[..8]}... - ${order.TotalAmount:F2} - {order.Status} - {order.ItemCount} items");
        }

        Console.WriteLine("\n📈 ACTUALIZANDO STOCK...");

        await updateStockHandler.HandleAsync(new UpdateProductStockCommand
        {
            ProductId = macbookId,
            NewQuantity = 100,
            Reason = "Reposición de inventario"
        });

        Console.WriteLine("\n📢 EVENTOS GENERADOS:");
        foreach (var evt in events)
        {
            Console.WriteLine($"   🔔 {evt.EventType} - {evt.OccurredAt:HH:mm:ss}");
        }

        Console.WriteLine($"\n✅ Demo completado. Total de eventos: {events.Count}");
    }
}

// Para ejecutar el demo:
// await CqrsDemo.RunDemo();
