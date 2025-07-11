using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

// ============================================================================
// SINGLETON PATTERN - IMPLEMENTACIONES REALISTAS Y THREAD-SAFE
// Ejemplo: Sistema de configuración y logger de aplicación
// ============================================================================

// ============================================================================
// 1. SINGLETON CLÁSICO THREAD-SAFE (Eager Initialization)
// ============================================================================
public sealed class ApplicationConfig
{
    private static readonly ApplicationConfig _instance = new ApplicationConfig();
    private readonly ConcurrentDictionary<string, string> _settings;
    private readonly DateTime _createdAt;

    // Constructor privado para prevenir instanciación externa
    private ApplicationConfig()
    {
        _settings = new ConcurrentDictionary<string, string>();
        _createdAt = DateTime.UtcNow;
        LoadDefaultSettings();
    }

    public static ApplicationConfig Instance => _instance;

    private void LoadDefaultSettings()
    {
        _settings["Environment"] = "Production";
        _settings["LogLevel"] = "Info";
        _settings["MaxConnections"] = "100";
        _settings["CacheExpiration"] = "3600";
        _settings["ApiTimeout"] = "30";
    }

    public string GetSetting(string key)
    {
        return _settings.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public void SetSetting(string key, string value)
    {
        _settings[key] = value;
        LogSettingChange(key, value);
    }

    public void ShowConfiguration()
    {
        Console.WriteLine($"\n🔧 CONFIGURACIÓN DE APLICACIÓN (Creada: {_createdAt:HH:mm:ss})");
        Console.WriteLine("========================================");
        foreach (var setting in _settings)
        {
            Console.WriteLine($"   {setting.Key}: {setting.Value}");
        }
    }

    private void LogSettingChange(string key, string value)
    {
        Console.WriteLine($"⚙️ Configuración actualizada: {key} = {value}");
    }
}

// ============================================================================
// 2. SINGLETON LAZY THREAD-SAFE (Lazy Initialization)
// ============================================================================
public sealed class DatabaseConnectionManager
{
    private static readonly Lazy<DatabaseConnectionManager> _instance = 
        new Lazy<DatabaseConnectionManager>(() => new DatabaseConnectionManager());

    private readonly ConcurrentDictionary<string, string> _connections;
    private readonly object _lockObject = new object();
    private int _activeConnections = 0;
    private readonly int _maxConnections = 10;

    private DatabaseConnectionManager()
    {
        _connections = new ConcurrentDictionary<string, string>();
        Console.WriteLine("🗄️ DatabaseConnectionManager inicializado");
    }

    public static DatabaseConnectionManager Instance => _instance.Value;

    public string GetConnection(string databaseName)
    {
        lock (_lockObject)
        {
            if (_activeConnections >= _maxConnections)
            {
                throw new InvalidOperationException($"Máximo de conexiones alcanzado: {_maxConnections}");
            }

            var connectionId = $"conn_{databaseName}_{Guid.NewGuid().ToString()[..8]}";
            _connections[connectionId] = $"Server=localhost;Database={databaseName};";
            _activeConnections++;

            Console.WriteLine($"🔌 Nueva conexión creada: {connectionId} (Total: {_activeConnections})");
            return connectionId;
        }
    }

    public void ReleaseConnection(string connectionId)
    {
        lock (_lockObject)
        {
            if (_connections.TryRemove(connectionId, out _))
            {
                _activeConnections--;
                Console.WriteLine($"🔌 Conexión liberada: {connectionId} (Total: {_activeConnections})");
            }
        }
    }

    public void ShowStatus()
    {
        Console.WriteLine($"\n📊 ESTADO DEL GESTOR DE CONEXIONES");
        Console.WriteLine($"   Conexiones activas: {_activeConnections}/{_maxConnections}");
        Console.WriteLine($"   Conexiones registradas: {_connections.Count}");
    }
}

// ============================================================================
// 3. SINGLETON CON DOUBLE-CHECKED LOCKING (Para casos especiales)
// ============================================================================
public sealed class CacheManager
{
    private static CacheManager _instance;
    private static readonly object _lock = new object();
    
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly Timer _cleanupTimer;

    private CacheManager()
    {
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        // Limpiar cache cada 60 segundos
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
        Console.WriteLine("🗂️ CacheManager inicializado con limpieza automática");
    }

    public static CacheManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new CacheManager();
                }
            }
            return _instance;
        }
    }

    public void Set(string key, object value, TimeSpan? expiration = null)
    {
        var expirationTime = expiration.HasValue 
            ? DateTime.UtcNow.Add(expiration.Value)
            : DateTime.UtcNow.AddHours(1); // Default 1 hora

        _cache[key] = new CacheEntry(value, expirationTime);
        Console.WriteLine($"💾 Cache guardado: {key} (Expira: {expirationTime:HH:mm:ss})");
    }

    public T Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpirationTime > DateTime.UtcNow)
            {
                Console.WriteLine($"✅ Cache hit: {key}");
                return (T)entry.Value;
            }
            else
            {
                _cache.TryRemove(key, out _);
                Console.WriteLine($"⏰ Cache expirado: {key}");
            }
        }
        
        Console.WriteLine($"❌ Cache miss: {key}");
        return default(T);
    }

    private void CleanupExpiredEntries(object state)
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.ExpirationTime <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        if (expiredKeys.Any())
        {
            Console.WriteLine($"🧹 Limpieza de cache: {expiredKeys.Count} entradas expiradas eliminadas");
        }
    }

    public void ShowCacheStatus()
    {
        var activeEntries = _cache.Count(kvp => kvp.Value.ExpirationTime > DateTime.UtcNow);
        Console.WriteLine($"\n🗂️ ESTADO DEL CACHE");
        Console.WriteLine($"   Entradas activas: {activeEntries}");
        Console.WriteLine($"   Entradas totales: {_cache.Count}");
    }

    private class CacheEntry
    {
        public object Value { get; }
        public DateTime ExpirationTime { get; }

        public CacheEntry(object value, DateTime expirationTime)
        {
            Value = value;
            ExpirationTime = expirationTime;
        }
    }
}

// ============================================================================
// 4. ANTI-PATRÓN: EJEMPLO DE SINGLETON MAL IMPLEMENTADO (NO USAR)
// ============================================================================
public class BadSingleton
{
    private static BadSingleton _instance;

    // ❌ No thread-safe, puede crear múltiples instancias
    public static BadSingleton Instance
    {
        get
        {
            if (_instance == null)
                _instance = new BadSingleton();
            return _instance;
        }
    }
}

// ============================================================================
// DEMO REALISTA DE SINGLETONS
// ============================================================================
public static class SingletonDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🏗️ DEMO: SINGLETON PATTERNS EN ACCIÓN");
        Console.WriteLine("====================================");

        // Demo 1: Application Configuration
        Console.WriteLine("\n1️⃣ CONFIGURACIÓN DE APLICACIÓN");
        var config1 = ApplicationConfig.Instance;
        var config2 = ApplicationConfig.Instance;
        
        Console.WriteLine($"   Misma instancia: {ReferenceEquals(config1, config2)}");
        
        config1.SetSetting("ApiUrl", "https://api.ejemplo.com");
        config2.ShowConfiguration();

        // Demo 2: Database Connection Manager
        Console.WriteLine("\n2️⃣ GESTOR DE CONEXIONES");
        var dbManager = DatabaseConnectionManager.Instance;
        
        var connections = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            connections.Add(dbManager.GetConnection($"Database{i + 1}"));
        }
        
        dbManager.ShowStatus();
        
        // Liberar algunas conexiones
        dbManager.ReleaseConnection(connections[0]);
        dbManager.ShowStatus();

        // Demo 3: Cache Manager
        Console.WriteLine("\n3️⃣ GESTOR DE CACHE");
        var cache = CacheManager.Instance;
        
        cache.Set("user:123", new { Name = "Juan", Email = "juan@ejemplo.com" }, TimeSpan.FromSeconds(5));
        cache.Set("product:456", new { Name = "Laptop", Price = 999.99 }, TimeSpan.FromSeconds(10));
        
        // Leer del cache inmediatamente
        var user = cache.Get<object>("user:123");
        var product = cache.Get<object>("product:456");
        
        cache.ShowCacheStatus();
        
        // Esperar a que expire el cache de usuario
        Console.WriteLine("\n⏳ Esperando 6 segundos para que expire el cache de usuario...");
        await Task.Delay(6000);
        
        var expiredUser = cache.Get<object>("user:123"); // Cache miss
        var validProduct = cache.Get<object>("product:456"); // Cache hit
        
        cache.ShowCacheStatus();

        // Demo 4: Test de concurrencia
        Console.WriteLine("\n4️⃣ TEST DE CONCURRENCIA");
        await TestConcurrency();

        Console.WriteLine("\n✅ Demo completado");
    }

    private static async Task TestConcurrency()
    {
        Console.WriteLine("   🔄 Creando 10 tareas concurrentes para obtener instancias...");
        
        var tasks = Enumerable.Range(1, 10).Select(async i =>
        {
            await Task.Delay(10); // Pequeña variación en timing
            var config = ApplicationConfig.Instance;
            var dbManager = DatabaseConnectionManager.Instance;
            var cache = CacheManager.Instance;
            
            return new { ConfigHash = config.GetHashCode(), DbHash = dbManager.GetHashCode(), CacheHash = cache.GetHashCode() };
        });

        var results = await Task.WhenAll(tasks);
        
        var uniqueConfigHashes = results.Select(r => r.ConfigHash).Distinct().Count();
        var uniqueDbHashes = results.Select(r => r.DbHash).Distinct().Count();
        var uniqueCacheHashes = results.Select(r => r.CacheHash).Distinct().Count();
        
        Console.WriteLine($"   ✅ Instancias únicas de ApplicationConfig: {uniqueConfigHashes}");
        Console.WriteLine($"   ✅ Instancias únicas de DatabaseConnectionManager: {uniqueDbHashes}");
        Console.WriteLine($"   ✅ Instancias únicas de CacheManager: {uniqueCacheHashes}");
        
        if (uniqueConfigHashes == 1 && uniqueDbHashes == 1 && uniqueCacheHashes == 1)
        {
            Console.WriteLine("   🎉 ¡Todos los Singletons son thread-safe!");
        }
    }
}

// Para ejecutar el demo:
// await SingletonDemo.RunDemo();
