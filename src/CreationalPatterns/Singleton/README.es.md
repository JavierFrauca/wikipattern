# Patrón Singleton

## 📋 Descripción

El **Patrón Singleton** asegura que una clase tenga solo una instancia mientras proporciona un punto de acceso global a esa instancia. Es uno de los patrones de diseño más utilizados y a veces controvertidos.

## 🎯 Propósito

- **Instancia única**: Garantizar que solo existe una instancia en toda la aplicación
- **Acceso global**: Proporcionar un punto de acceso global a esa instancia
- **Inicialización perezosa**: Crear la instancia solo cuando sea necesaria
- **Seguridad de hilos**: Asegurar la creación segura en entornos multi-hilo

## ✅ Cuándo Usar

- **Gestión de configuración**: Configuraciones y ajustes de la aplicación
- **Servicios de logging**: Logging centralizado en toda la aplicación
- **Conexiones de base de datos**: Gestión de pool de conexiones
- **Servicios de caché**: Gestión global de caché
- **Interfaces de hardware**: Spoolers de impresora, drivers de dispositivos

## ❌ Cuándo NO Usar

- **Dificultades de testing**: Difícil de mockear y testear unitariamente
- **Dependencias ocultas**: Hace las dependencias menos obvias
- **Acoplamiento fuerte**: Crea estado global implícito
- **Problemas de escalabilidad**: Puede convertirse en un cuello de botella
- **Violación del SRP**: A menudo hace demasiadas cosas

## 🏗️ Estructura

```mermaid
classDiagram
    class Singleton {
        -instance: Singleton
        -Singleton()
        +GetInstance(): Singleton
        +DoSomething(): void
    }
    
    note for Singleton "Constructor privado previene instanciación directa"
```

## 💡 Variaciones de Implementación

### 1. **Inicialización Perezosa Thread-Safe (Recomendada)**

```csharp
public sealed class Singleton
{
    private static readonly Lazy<Singleton> _lazy = new(() => new Singleton());
    
    private Singleton() 
    {
        // Constructor privado previene instanciación externa
    }
    
    public static Singleton Instance => _lazy.Value;
    
    public void DoSomething()
    {
        Console.WriteLine("La instancia Singleton está funcionando...");
    }
}

// Uso
var instance1 = Singleton.Instance;
var instance2 = Singleton.Instance;
Console.WriteLine(ReferenceEquals(instance1, instance2)); // True
```

### 2. **Double-Checked Locking**

```csharp
public sealed class ThreadSafeSingleton
{
    private static ThreadSafeSingleton _instance;
    private static readonly object _lock = new object();
    
    private ThreadSafeSingleton() { }
    
    public static ThreadSafeSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new ThreadSafeSingleton();
                }
            }
            return _instance;
        }
    }
}
```

### 3. **Constructor Estático (Thread-Safe)**

```csharp
public sealed class StaticSingleton
{
    private static readonly StaticSingleton _instance = new StaticSingleton();
    
    // Constructor estático explícito previene carga perezosa
    static StaticSingleton() { }
    
    private StaticSingleton() { }
    
    public static StaticSingleton Instance => _instance;
}
```

## 📊 Ejemplo del Mundo Real: Gestor de Configuración

```csharp
public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _lazy = new(() => new ConfigurationManager());
    private readonly Dictionary<string, string> _settings;
    private readonly string _configFilePath;
    
    private ConfigurationManager()
    {
        _configFilePath = "appsettings.json";
        _settings = LoadConfiguration();
    }
    
    public static ConfigurationManager Instance => _lazy.Value;
    
    public string GetSetting(string key)
    {
        return _settings.TryGetValue(key, out var value) ? value : string.Empty;
    }
    
    public void SetSetting(string key, string value)
    {
        _settings[key] = value;
        SaveConfiguration();
    }
    
    public T GetSetting<T>(string key)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        return default(T);
    }
    
    private Dictionary<string, string> LoadConfiguration()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando configuración: {ex.Message}");
        }
        
        return new Dictionary<string, string>();
    }
    
    private void SaveConfiguration()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error guardando configuración: {ex.Message}");
        }
    }
}

// Uso
var config = ConfigurationManager.Instance;
config.SetSetting("DatabaseUrl", "Server=localhost;Database=MyApp");
var dbUrl = config.GetSetting("DatabaseUrl");
```

## 🎯 Ejemplo: Servicio de Logger

```csharp
public sealed class Logger
{
    private static readonly Lazy<Logger> _lazy = new(() => new Logger());
    private readonly string _logFilePath;
    private readonly object _fileLock = new object();
    
    private Logger()
    {
        _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");
    }
    
    public static Logger Instance => _lazy.Value;
    
    public void LogInfo(string message)
    {
        WriteLog("INFO", message);
    }
    
    public void LogError(string message, Exception exception = null)
    {
        var fullMessage = exception != null ? $"{message} - {exception}" : message;
        WriteLog("ERROR", fullMessage);
    }
    
    public void LogWarning(string message)
    {
        WriteLog("WARNING", message);
    }
    
    private void WriteLog(string level, string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        
        lock (_fileLock)
        {
            try
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error escribiendo al archivo de log: {ex.Message}");
            }
        }
        
        // También log en consola
        Console.WriteLine(logEntry);
    }
}

// Uso en toda la aplicación
Logger.Instance.LogInfo("Aplicación iniciada");
Logger.Instance.LogError("Conexión a base de datos falló", new SQLException("Connection timeout"));
```

## 🔧 Alternativas Modernas

### 1. **Contenedor de Inyección de Dependencias**

```csharp
// En lugar del patrón Singleton, usar contenedor DI
services.AddSingleton<IConfigurationService, ConfigurationService>();
services.AddSingleton<ILogger, Logger>();

public class OrderService
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _config;
    
    public OrderService(ILogger logger, IConfigurationService config)
    {
        _logger = logger;
        _config = config;
    }
}
```

### 2. **Clase Estática (para operaciones sin estado)**

```csharp
public static class MathUtils
{
    public static double CalculateDistance(Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }
    
    public static bool IsPrime(int number)
    {
        if (number < 2) return false;
        for (int i = 2; i <= Math.Sqrt(number); i++)
        {
            if (number % i == 0) return false;
        }
        return true;
    }
}
```

## ⚡ Consideraciones de Rendimiento

- **Uso de memoria**: Las instancias Singleton viven durante toda la vida de la aplicación
- **Contención de hilos**: Múltiples hilos accediendo a la misma instancia
- **Costo de inicialización**: La inicialización costosa debería ser perezosa
- **Recolección de basura**: No puede ser recolectado hasta que termine la aplicación

## 🧪 Desafíos de Testing y Soluciones

```csharp
// Problema: Difícil de testear debido al estado global
public class OrderService
{
    public void ProcessOrder(Order order)
    {
        Logger.Instance.LogInfo($"Procesando pedido {order.Id}");
        // Difícil verificar logging en tests
    }
}

// Solución: Usar inyección de dependencias
public class OrderService
{
    private readonly ILogger _logger;
    
    public OrderService(ILogger logger)
    {
        _logger = logger;
    }
    
    public void ProcessOrder(Order order)
    {
        _logger.LogInfo($"Procesando pedido {order.Id}");
        // Fácil mockear logger en tests
    }
}

[Test]
public void ProcessOrder_ShouldLogOrderId()
{
    // Arrange
    var mockLogger = new Mock<ILogger>();
    var orderService = new OrderService(mockLogger.Object);
    var order = new Order { Id = 123 };
    
    // Act
    orderService.ProcessOrder(order);
    
    // Assert
    mockLogger.Verify(l => l.LogInfo("Procesando pedido 123"), Times.Once);
}
```

## 📊 Métricas y Monitoreo

```csharp
public sealed class PerformanceCounter
{
    private static readonly Lazy<PerformanceCounter> _lazy = new(() => new PerformanceCounter());
    private readonly Dictionary<string, long> _counters = new();
    private readonly object _lock = new object();
    
    public static PerformanceCounter Instance => _lazy.Value;
    
    public void Increment(string counterName)
    {
        lock (_lock)
        {
            _counters[counterName] = _counters.GetValueOrDefault(counterName, 0) + 1;
        }
    }
    
    public long GetCount(string counterName)
    {
        lock (_lock)
        {
            return _counters.GetValueOrDefault(counterName, 0);
        }
    }
    
    public Dictionary<string, long> GetAllCounters()
    {
        lock (_lock)
        {
            return new Dictionary<string, long>(_counters);
        }
    }
}
```

## 🔗 Patrones Relacionados

- **[Factory Method](../FactoryMethod/)**: Puede usarse para crear instancias Singleton
- **[Abstract Factory](../AbstractFactory/)**: A menudo implementadas como Singletons
- **[Builder](../Builder/)**: Las instancias Builder a veces son Singletons
- **[Facade](../../StructuralPatterns/Facade/)**: Las fachadas comúnmente se implementan como Singletons

## 📚 Recursos Adicionales

- [Microsoft: Patrón Singleton](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff650316(v=pandp.10))
- [Inyección de Dependencias vs Singleton](https://martinfowler.com/articles/injection.html)
- [Por qué los Singletons son Controvertidos](https://stackoverflow.com/questions/137975/what-is-so-bad-about-singletons)

---

> ⚠️ **Advertencia**: Aunque Singleton puede ser útil, considera usar contenedores de Inyección de Dependencias en su lugar para mejor testabilidad y mantenibilidad en aplicaciones modernas.
