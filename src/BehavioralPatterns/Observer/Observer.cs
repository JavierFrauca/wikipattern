using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ============================================================================
// OBSERVER PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de trading de acciones en tiempo real
// ============================================================================

// Datos del evento
public class StockPriceChangedEventArgs : EventArgs
{
    public string Symbol { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }
    public decimal ChangePercent { get; }
    public DateTime Timestamp { get; }
    public long Volume { get; }

    public StockPriceChangedEventArgs(string symbol, decimal oldPrice, decimal newPrice, long volume)
    {
        Symbol = symbol;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        ChangePercent = oldPrice > 0 ? ((newPrice - oldPrice) / oldPrice) * 100 : 0;
        Timestamp = DateTime.UtcNow;
        Volume = volume;
    }
}

// Observer interface mejorado
public interface IStockObserver
{
    string Name { get; }
    Task OnStockPriceChangedAsync(StockPriceChangedEventArgs eventArgs);
    bool IsActive { get; set; }
}

// Subject (Observable) - Bolsa de valores
public class StockExchange
{
    private readonly Dictionary<string, decimal> _stockPrices;
    private readonly List<IStockObserver> _observers;
    private readonly object _lock = new object();

    public StockExchange()
    {
        _stockPrices = new Dictionary<string, decimal>();
        _observers = new List<IStockObserver>();
    }

    // Gestión de observadores
    public void Subscribe(IStockObserver observer)
    {
        lock (_lock)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                Console.WriteLine($"📈 {observer.Name} se suscribió a actualizaciones de precios");
            }
        }
    }

    public void Unsubscribe(IStockObserver observer)
    {
        lock (_lock)
        {
            if (_observers.Remove(observer))
            {
                Console.WriteLine($"📉 {observer.Name} canceló su suscripción");
            }
        }
    }

    // Actualizar precio y notificar
    public async Task UpdateStockPriceAsync(string symbol, decimal newPrice, long volume = 1000)
    {
        decimal oldPrice = GetCurrentPrice(symbol);
        
        lock (_lock)
        {
            _stockPrices[symbol] = newPrice;
        }

        var eventArgs = new StockPriceChangedEventArgs(symbol, oldPrice, newPrice, volume);
        
        Console.WriteLine($"\n🔔 ACTUALIZACIÓN DE PRECIO: {symbol}");
        Console.WriteLine($"   Precio anterior: ${oldPrice:F2}");
        Console.WriteLine($"   Precio nuevo: ${newPrice:F2}");
        Console.WriteLine($"   Cambio: {(eventArgs.ChangePercent >= 0 ? "+" : "")}{eventArgs.ChangePercent:F2}%");
        Console.WriteLine($"   Volumen: {volume:N0} acciones");

        await NotifyObserversAsync(eventArgs);
    }

    private async Task NotifyObserversAsync(StockPriceChangedEventArgs eventArgs)
    {
        IStockObserver[] activeObservers;
        
        lock (_lock)
        {
            activeObservers = _observers.Where(o => o.IsActive).ToArray();
        }

        if (!activeObservers.Any())
        {
            Console.WriteLine("   ⚠️ No hay observadores activos");
            return;
        }

        Console.WriteLine($"   📢 Notificando a {activeObservers.Length} observadores...");

        // Notificar a todos los observadores en paralelo
        var tasks = activeObservers.Select(observer => NotifyObserverSafelyAsync(observer, eventArgs));
        await Task.WhenAll(tasks);
    }

    private async Task NotifyObserverSafelyAsync(IStockObserver observer, StockPriceChangedEventArgs eventArgs)
    {
        try
        {
            await observer.OnStockPriceChangedAsync(eventArgs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error notificando a {observer.Name}: {ex.Message}");
        }
    }

    public decimal GetCurrentPrice(string symbol)
    {
        lock (_lock)
        {
            return _stockPrices.TryGetValue(symbol, out var price) ? price : 0;
        }
    }

    public Dictionary<string, decimal> GetAllPrices()
    {
        lock (_lock)
        {
            return new Dictionary<string, decimal>(_stockPrices);
        }
    }
}

// ============================================================================
// OBSERVADORES CONCRETOS - DIFERENTES TIPOS DE INVERSORES
// ============================================================================

// Trader de alta frecuencia
public class HighFrequencyTrader : IStockObserver
{
    public string Name { get; }
    public bool IsActive { get; set; } = true;
    private readonly decimal _triggerPercentage;

    public HighFrequencyTrader(string name, decimal triggerPercentage = 0.1m)
    {
        Name = name;
        _triggerPercentage = triggerPercentage;
    }

    public async Task OnStockPriceChangedAsync(StockPriceChangedEventArgs eventArgs)
    {
        await Task.Delay(50); // Simular procesamiento rápido

        if (Math.Abs(eventArgs.ChangePercent) >= _triggerPercentage)
        {
            var action = eventArgs.ChangePercent > 0 ? "COMPRAR" : "VENDER";
            Console.WriteLine($"   🤖 {Name}: {action} {eventArgs.Symbol} - Cambio significativo detectado ({eventArgs.ChangePercent:F2}%)");
        }
    }
}

// Inversor conservador
public class ConservativeInvestor : IStockObserver
{
    public string Name { get; }
    public bool IsActive { get; set; } = true;
    private readonly List<string> _watchlist;

    public ConservativeInvestor(string name, params string[] watchlist)
    {
        Name = name;
        _watchlist = new List<string>(watchlist);
    }

    public async Task OnStockPriceChangedAsync(StockPriceChangedEventArgs eventArgs)
    {
        await Task.Delay(200); // Simular análisis más lento

        if (_watchlist.Contains(eventArgs.Symbol))
        {
            if (eventArgs.ChangePercent <= -5)
            {
                Console.WriteLine($"   💰 {Name}: Oportunidad de compra en {eventArgs.Symbol} - Caída del {Math.Abs(eventArgs.ChangePercent):F2}%");
            }
            else if (eventArgs.ChangePercent >= 10)
            {
                Console.WriteLine($"   💼 {Name}: Considerar venta de {eventArgs.Symbol} - Ganancia del {eventArgs.ChangePercent:F2}%");
            }
        }
    }
}

// Sistema de alertas de riesgo
public class RiskManagementSystem : IStockObserver
{
    public string Name { get; } = "Sistema de Gestión de Riesgo";
    public bool IsActive { get; set; } = true;
    private readonly decimal _criticalThreshold;

    public RiskManagementSystem(decimal criticalThreshold = 15m)
    {
        _criticalThreshold = criticalThreshold;
    }

    public async Task OnStockPriceChangedAsync(StockPriceChangedEventArgs eventArgs)
    {
        await Task.Delay(100);

        if (Math.Abs(eventArgs.ChangePercent) >= _criticalThreshold)
        {
            Console.WriteLine($"   🚨 {Name}: ALERTA CRÍTICA - {eventArgs.Symbol} ha cambiado {eventArgs.ChangePercent:F2}%");
            Console.WriteLine($"      ⚠️ Requiere intervención inmediata del equipo de riesgo");
        }
    }
}

// Analizador de mercado
public class MarketAnalyzer : IStockObserver
{
    public string Name { get; } = "Analizador de Mercado";
    public bool IsActive { get; set; } = true;
    private readonly Dictionary<string, List<decimal>> _priceHistory;

    public MarketAnalyzer()
    {
        _priceHistory = new Dictionary<string, List<decimal>>();
    }

    public async Task OnStockPriceChangedAsync(StockPriceChangedEventArgs eventArgs)
    {
        await Task.Delay(150);

        // Mantener historial de precios
        if (!_priceHistory.ContainsKey(eventArgs.Symbol))
        {
            _priceHistory[eventArgs.Symbol] = new List<decimal>();
        }

        _priceHistory[eventArgs.Symbol].Add(eventArgs.NewPrice);

        // Mantener solo los últimos 10 precios
        if (_priceHistory[eventArgs.Symbol].Count > 10)
        {
            _priceHistory[eventArgs.Symbol].RemoveAt(0);
        }

        // Análisis de tendencia
        if (_priceHistory[eventArgs.Symbol].Count >= 3)
        {
            var prices = _priceHistory[eventArgs.Symbol];
            var trend = AnalyzeTrend(prices);
            Console.WriteLine($"   📊 {Name}: Tendencia de {eventArgs.Symbol} → {trend}");
        }
    }

    private string AnalyzeTrend(List<decimal> prices)
    {
        if (prices.Count < 3) return "Insuficientes datos";

        var recent = prices.TakeLast(3).ToList();
        if (recent[2] > recent[1] && recent[1] > recent[0])
            return "🔼 ALCISTA";
        else if (recent[2] < recent[1] && recent[1] < recent[0])
            return "🔽 BAJISTA";
        else
            return "➡️ LATERAL";
    }
}

// ============================================================================
// DEMO REALISTA
// ============================================================================
public static class StockTradingDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("📈 DEMO: SISTEMA DE TRADING CON OBSERVER PATTERN");
        Console.WriteLine("================================================");

        // Crear la bolsa de valores
        var exchange = new StockExchange();

        // Crear diferentes tipos de observadores
        var hfTrader1 = new HighFrequencyTrader("HF-Bot-Alpha", 0.5m);
        var hfTrader2 = new HighFrequencyTrader("HF-Bot-Beta", 1.0m);
        var conservativeInvestor = new ConservativeInvestor("Warren Investor", "AAPL", "MSFT", "GOOGL");
        var riskManager = new RiskManagementSystem(10m);
        var marketAnalyzer = new MarketAnalyzer();

        // Suscribir observadores
        exchange.Subscribe(hfTrader1);
        exchange.Subscribe(hfTrader2);
        exchange.Subscribe(conservativeInvestor);
        exchange.Subscribe(riskManager);
        exchange.Subscribe(marketAnalyzer);

        // Simular movimientos de mercado
        var marketMovements = new[]
        {
            ("AAPL", 150.00m, 2000L),
            ("AAPL", 151.50m, 3500L),   // +1% - Movimiento normal
            ("MSFT", 300.00m, 1500L),
            ("AAPL", 148.25m, 5000L),   // -2.17% - Caída moderada
            ("GOOGL", 2800.00m, 800L),
            ("AAPL", 135.75m, 8000L),   // -9.5% - Caída significativa
            ("MSFT", 285.00m, 4000L),   // -5% - Oportunidad de compra
            ("AAPL", 149.20m, 6000L),   // +9.9% - Recuperación
        };

        Console.WriteLine("\n🎯 Iniciando simulación de trading...\n");

        foreach (var (symbol, price, volume) in marketMovements)
        {
            await exchange.UpdateStockPriceAsync(symbol, price, volume);
            await Task.Delay(3000); // Pausa entre actualizaciones
        }

        // Desactivar un observador durante la ejecución
        Console.WriteLine("\n🔄 Desactivando HF-Bot-Beta...");
        hfTrader2.IsActive = false;

        // Más movimientos
        await exchange.UpdateStockPriceAsync("AAPL", 165.00m, 10000L); // +10.5% - Gran subida

        Console.WriteLine("\n📊 Resumen final de precios:");
        foreach (var kvp in exchange.GetAllPrices())
        {
            Console.WriteLine($"   {kvp.Key}: ${kvp.Value:F2}");
        }

        Console.WriteLine("\n✅ Demo completado");
    }
}

// Para ejecutar el demo:
// await StockTradingDemo.RunDemo();
