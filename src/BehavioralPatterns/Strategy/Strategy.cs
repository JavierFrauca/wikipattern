using System;
using System.Collections.Generic;
using System.Linq;

// ============================================================================
// STRATEGY PATTERN - IMPLEMENTACIÓN DIDÁCTICA
// Ejemplo: Sistema de cálculo de precios con diferentes estrategias de descuento
// ============================================================================

// ============================================================================
// MODELOS DE DATOS
// ============================================================================
public class Product
{
    public string Name { get; set; }
    public decimal BasePrice { get; set; }
    public string Category { get; set; }
    public bool IsPremium { get; set; }
    public DateTime ReleaseDate { get; set; }

    public override string ToString()
    {
        return $"{Name} ({Category}) - ${BasePrice:F2}";
    }
}

public class Customer
{
    public string Name { get; set; }
    public CustomerType Type { get; set; }
    public DateTime MemberSince { get; set; }
    public decimal TotalPurchases { get; set; }
    public bool HasPremiumMembership { get; set; }

    public int YearsAsMember => DateTime.Now.Year - MemberSince.Year;
}

public enum CustomerType
{
    Regular,
    VIP,
    Employee,
    Student,
    Senior
}

public class PricingResult
{
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string DiscountDescription { get; set; }
    public List<string> AppliedDiscounts { get; set; } = new List<string>();

    public decimal DiscountPercentage => OriginalPrice > 0 ? (DiscountAmount / OriginalPrice) * 100 : 0;

    public void ShowBreakdown()
    {
        Console.WriteLine($"   💰 Precio Original: ${OriginalPrice:F2}");
        if (DiscountAmount > 0)
        {
            Console.WriteLine($"   🎯 Descuento Total: ${DiscountAmount:F2} ({DiscountPercentage:F1}%)");
            foreach (var discount in AppliedDiscounts)
            {
                Console.WriteLine($"      • {discount}");
            }
        }
        Console.WriteLine($"   ✅ Precio Final: ${FinalPrice:F2}");
    }
}

// ============================================================================
// INTERFAZ DE ESTRATEGIA
// ============================================================================
public interface IPricingStrategy
{
    string StrategyName { get; }
    PricingResult CalculatePrice(Product product, Customer customer, int quantity = 1);
    bool IsApplicable(Product product, Customer customer);
}

// ============================================================================
// ESTRATEGIAS CONCRETAS DE PRECIOS
// ============================================================================

// Estrategia de precio estándar (sin descuentos)
public class StandardPricingStrategy : IPricingStrategy
{
    public string StrategyName => "Precio Estándar";

    public PricingResult CalculatePrice(Product product, Customer customer, int quantity = 1)
    {
        var originalPrice = product.BasePrice * quantity;
        
        return new PricingResult
        {
            OriginalPrice = originalPrice,
            DiscountAmount = 0,
            FinalPrice = originalPrice,
            DiscountDescription = "Sin descuentos aplicables",
            AppliedDiscounts = new List<string> { "Precio estándar" }
        };
    }

    public bool IsApplicable(Product product, Customer customer) => true;
}

// Estrategia VIP con descuentos especiales
public class VIPPricingStrategy : IPricingStrategy
{
    public string StrategyName => "Precios VIP";

    public PricingResult CalculatePrice(Product product, Customer customer, int quantity = 1)
    {
        var originalPrice = product.BasePrice * quantity;
        var discountAmount = 0m;
        var appliedDiscounts = new List<string>();

        // Descuento base VIP
        var vipDiscount = originalPrice * 0.15m; // 15%
        discountAmount += vipDiscount;
        appliedDiscounts.Add("Descuento VIP: 15%");

        // Descuento adicional por antigüedad
        if (customer.YearsAsMember >= 5)
        {
            var loyaltyDiscount = originalPrice * 0.05m; // 5% adicional
            discountAmount += loyaltyDiscount;
            appliedDiscounts.Add($"Descuento por lealtad ({customer.YearsAsMember} años): 5%");
        }

        // Descuento por volumen de compras
        if (customer.TotalPurchases > 10000)
        {
            var volumeDiscount = originalPrice * 0.03m; // 3% adicional
            discountAmount += volumeDiscount;
            appliedDiscounts.Add("Descuento por volumen de compras: 3%");
        }

        return new PricingResult
        {
            OriginalPrice = originalPrice,
            DiscountAmount = discountAmount,
            FinalPrice = originalPrice - discountAmount,
            DiscountDescription = $"Descuentos VIP aplicados",
            AppliedDiscounts = appliedDiscounts
        };
    }

    public bool IsApplicable(Product product, Customer customer) => customer.Type == CustomerType.VIP;
}

// Estrategia para empleados
public class EmployeePricingStrategy : IPricingStrategy
{
    public string StrategyName => "Precios de Empleado";

    public PricingResult CalculatePrice(Product product, Customer customer, int quantity = 1)
    {
        var originalPrice = product.BasePrice * quantity;
        var discountAmount = originalPrice * 0.30m; // 30% de descuento
        
        return new PricingResult
        {
            OriginalPrice = originalPrice,
            DiscountAmount = discountAmount,
            FinalPrice = originalPrice - discountAmount,
            DiscountDescription = "Descuento especial para empleados",
            AppliedDiscounts = new List<string> { "Descuento de empleado: 30%" }
        };
    }

    public bool IsApplicable(Product product, Customer customer) => customer.Type == CustomerType.Employee;
}

// Estrategia para estudiantes
public class StudentPricingStrategy : IPricingStrategy
{
    public string StrategyName => "Precios para Estudiantes";

    public PricingResult CalculatePrice(Product product, Customer customer, int quantity = 1)
    {
        var originalPrice = product.BasePrice * quantity;
        var appliedDiscounts = new List<string>();
        var discountAmount = 0m;

        // Solo ciertos productos tienen descuento estudiantil
        if (product.Category == "Software" || product.Category == "Libros")
        {
            discountAmount = originalPrice * 0.20m; // 20% de descuento
            appliedDiscounts.Add($"Descuento estudiantil en {product.Category}: 20%");
        }
        else
        {
            discountAmount = originalPrice * 0.05m; // 5% descuento general
            appliedDiscounts.Add("Descuento estudiantil general: 5%");
        }

        return new PricingResult
        {
            OriginalPrice = originalPrice,
            DiscountAmount = discountAmount,
            FinalPrice = originalPrice - discountAmount,
            DiscountDescription = "Descuentos para estudiantes aplicados",
            AppliedDiscounts = appliedDiscounts
        };
    }

    public bool IsApplicable(Product product, Customer customer) => customer.Type == CustomerType.Student;
}

// Estrategia de descuentos por temporada
public class SeasonalPricingStrategy : IPricingStrategy
{
    private readonly string _seasonName;
    private readonly decimal _discountPercentage;
    private readonly List<string> _applicableCategories;

    public string StrategyName => $"Oferta de {_seasonName}";

    public SeasonalPricingStrategy(string seasonName, decimal discountPercentage, params string[] categories)
    {
        _seasonName = seasonName;
        _discountPercentage = discountPercentage;
        _applicableCategories = categories?.ToList() ?? new List<string>();
    }

    public PricingResult CalculatePrice(Product product, Customer customer, int quantity = 1)
    {
        var originalPrice = product.BasePrice * quantity;
        var discountAmount = 0m;
        var appliedDiscounts = new List<string>();

        if (_applicableCategories.Contains(product.Category))
        {
            discountAmount = originalPrice * (_discountPercentage / 100);
            appliedDiscounts.Add($"Oferta de {_seasonName}: {_discountPercentage}% en {product.Category}");
        }

        return new PricingResult
        {
            OriginalPrice = originalPrice,
            DiscountAmount = discountAmount,
            FinalPrice = originalPrice - discountAmount,
            DiscountDescription = $"Oferta especial de {_seasonName}",
            AppliedDiscounts = appliedDiscounts
        };
    }

    public bool IsApplicable(Product product, Customer customer)
    {
        return _applicableCategories.Contains(product.Category);
    }
}

// Estrategia de descuentos por cantidad
public class BulkPricingStrategy : IPricingStrategy
{
    public string StrategyName => "Precios por Volumen";

    public PricingResult CalculatePrice(Product product, Customer customer, int quantity = 1)
    {
        var originalPrice = product.BasePrice * quantity;
        var discountAmount = 0m;
        var appliedDiscounts = new List<string>();

        // Descuentos escalonados por cantidad
        if (quantity >= 100)
        {
            discountAmount = originalPrice * 0.20m; // 20% para 100+
            appliedDiscounts.Add($"Descuento por volumen (100+ unidades): 20%");
        }
        else if (quantity >= 50)
        {
            discountAmount = originalPrice * 0.15m; // 15% para 50+
            appliedDiscounts.Add($"Descuento por volumen (50+ unidades): 15%");
        }
        else if (quantity >= 20)
        {
            discountAmount = originalPrice * 0.10m; // 10% para 20+
            appliedDiscounts.Add($"Descuento por volumen (20+ unidades): 10%");
        }
        else if (quantity >= 10)
        {
            discountAmount = originalPrice * 0.05m; // 5% para 10+
            appliedDiscounts.Add($"Descuento por volumen (10+ unidades): 5%");
        }

        return new PricingResult
        {
            OriginalPrice = originalPrice,
            DiscountAmount = discountAmount,
            FinalPrice = originalPrice - discountAmount,
            DiscountDescription = "Descuentos por cantidad aplicados",
            AppliedDiscounts = appliedDiscounts
        };
    }

    public bool IsApplicable(Product product, Customer customer) => true;
}

// ============================================================================
// CONTEXTO - GESTOR DE PRECIOS
// ============================================================================
public class PriceCalculator
{
    private readonly List<IPricingStrategy> _strategies;
    private IPricingStrategy _defaultStrategy;

    public PriceCalculator()
    {
        _strategies = new List<IPricingStrategy>();
        _defaultStrategy = new StandardPricingStrategy();
    }

    public void AddStrategy(IPricingStrategy strategy)
    {
        _strategies.Add(strategy);
        Console.WriteLine($"➕ Estrategia agregada: {strategy.StrategyName}");
    }

    public void SetDefaultStrategy(IPricingStrategy strategy)
    {
        _defaultStrategy = strategy;
        Console.WriteLine($"🔧 Estrategia por defecto: {strategy.StrategyName}");
    }

    // Calcular precio usando la mejor estrategia disponible
    public PricingResult CalculateBestPrice(Product product, Customer customer, int quantity = 1)
    {
        Console.WriteLine($"\n🔍 CALCULANDO MEJOR PRECIO PARA {customer.Name}");
        Console.WriteLine($"   📦 Producto: {product}");
        Console.WriteLine($"   👤 Cliente: {customer.Type} (miembro desde {customer.MemberSince.Year})");
        Console.WriteLine($"   📊 Cantidad: {quantity} unidades");

        var applicableStrategies = _strategies.Where(s => s.IsApplicable(product, customer)).ToList();
        
        if (!applicableStrategies.Any())
        {
            Console.WriteLine($"   🔄 Usando estrategia por defecto: {_defaultStrategy.StrategyName}");
            return _defaultStrategy.CalculatePrice(product, customer, quantity);
        }

        // Evaluar todas las estrategias aplicables y elegir la mejor para el cliente
        var results = applicableStrategies
            .Select(strategy => new { Strategy = strategy, Result = strategy.CalculatePrice(product, customer, quantity) })
            .OrderBy(x => x.Result.FinalPrice) // Ordenar por precio final (menor es mejor para el cliente)
            .ToList();

        var bestOption = results.First();
        Console.WriteLine($"   ✅ Mejor estrategia encontrada: {bestOption.Strategy.StrategyName}");
        
        return bestOption.Result;
    }

    // Comparar todas las estrategias disponibles
    public void CompareAllStrategies(Product product, Customer customer, int quantity = 1)
    {
        Console.WriteLine($"\n📊 COMPARACIÓN DE TODAS LAS ESTRATEGIAS");
        Console.WriteLine("======================================");

        var allStrategies = new List<IPricingStrategy>(_strategies) { _defaultStrategy };
        
        foreach (var strategy in allStrategies)
        {
            if (strategy.IsApplicable(product, customer))
            {
                Console.WriteLine($"\n🎯 {strategy.StrategyName}:");
                var result = strategy.CalculatePrice(product, customer, quantity);
                result.ShowBreakdown();
            }
            else
            {
                Console.WriteLine($"\n❌ {strategy.StrategyName}: No aplicable");
            }
        }
    }

    public void ShowAvailableStrategies()
    {
        Console.WriteLine($"\n📋 ESTRATEGIAS DISPONIBLES");
        Console.WriteLine("=========================");
        Console.WriteLine($"🔧 Por defecto: {_defaultStrategy.StrategyName}");
        
        for (int i = 0; i < _strategies.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_strategies[i].StrategyName}");
        }
    }
}

// ============================================================================
// DEMO DIDÁCTICO
// ============================================================================
public static class StrategyPatternDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("💰 DEMO: STRATEGY PATTERN - SISTEMA DE PRECIOS DINÁMICO");
        Console.WriteLine("========================================================");

        // Configurar el calculador de precios
        var priceCalculator = new PriceCalculator();
        
        // Agregar diferentes estrategias
        priceCalculator.AddStrategy(new VIPPricingStrategy());
        priceCalculator.AddStrategy(new EmployeePricingStrategy());
        priceCalculator.AddStrategy(new StudentPricingStrategy());
        priceCalculator.AddStrategy(new SeasonalPricingStrategy("Black Friday", 25, "Electrónicos", "Software"));
        priceCalculator.AddStrategy(new BulkPricingStrategy());

        priceCalculator.ShowAvailableStrategies();

        // Crear productos de ejemplo
        var products = new[]
        {
            new Product { Name = "Laptop Gaming", BasePrice = 1299.99m, Category = "Electrónicos", IsPremium = true },
            new Product { Name = "Office 365", BasePrice = 99.99m, Category = "Software", IsPremium = false },
            new Product { Name = "Programming Book", BasePrice = 49.99m, Category = "Libros", IsPremium = false }
        };

        // Crear clientes de ejemplo
        var customers = new[]
        {
            new Customer 
            { 
                Name = "Juan Pérez", 
                Type = CustomerType.VIP, 
                MemberSince = new DateTime(2018, 1, 1), 
                TotalPurchases = 15000,
                HasPremiumMembership = true
            },
            new Customer 
            { 
                Name = "María García", 
                Type = CustomerType.Student, 
                MemberSince = new DateTime(2023, 9, 1), 
                TotalPurchases = 500 
            },
            new Customer 
            { 
                Name = "Carlos López", 
                Type = CustomerType.Employee, 
                MemberSince = new DateTime(2020, 3, 15), 
                TotalPurchases = 8000 
            },
            new Customer 
            { 
                Name = "Ana Rodríguez", 
                Type = CustomerType.Regular, 
                MemberSince = new DateTime(2022, 6, 1), 
                TotalPurchases = 2000 
            }
        };

        // Escenarios de demostración
        var scenarios = new[]
        {
            new { Customer = customers[0], Product = products[0], Quantity = 1 }, // VIP comprando laptop
            new { Customer = customers[1], Product = products[1], Quantity = 1 }, // Estudiante comprando software
            new { Customer = customers[2], Product = products[2], Quantity = 1 }, // Empleado comprando libro
            new { Customer = customers[3], Product = products[0], Quantity = 25 }, // Cliente regular comprando en volumen
        };

        // Demostrar cálculo de precios para cada escenario
        foreach (var scenario in scenarios)
        {
            var result = priceCalculator.CalculateBestPrice(
                scenario.Product, 
                scenario.Customer, 
                scenario.Quantity
            );
            
            Console.WriteLine($"\n💳 RESULTADO FINAL:");
            result.ShowBreakdown();
            
            Console.WriteLine("\n" + new string('=', 60));
        }

        // Demostración adicional: Comparar estrategias para un caso específico
        Console.WriteLine($"\n🔍 ANÁLISIS DETALLADO: Comparando estrategias para cliente VIP");
        priceCalculator.CompareAllStrategies(products[0], customers[0], 1);

        Console.WriteLine("\n✅ Demo del Strategy Pattern completado");
        Console.WriteLine("\n💡 LECCIONES APRENDIDAS:");
        Console.WriteLine("   • Strategy permite cambiar algoritmos dinámicamente");
        Console.WriteLine("   • Cada estrategia encapsula una lógica de negocio específica");
        Console.WriteLine("   • El contexto puede elegir automáticamente la mejor estrategia");
        Console.WriteLine("   • Fácil agregar nuevas estrategias sin modificar código existente");
        Console.WriteLine("   • Ideal para sistemas con múltiples formas de realizar la misma tarea");
    }
}

// Para ejecutar el demo:
// StrategyPatternDemo.RunDemo();
