using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Text.Json;

// ============================================================================
// SIGNALR PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de notificaciones en tiempo real para e-commerce
// ============================================================================

// ============================================================================
// CONNECTION MANAGEMENT - GESTIÓN DE CONEXIONES
// ============================================================================

/// <summary>
/// Información de una conexión
/// </summary>
public class ConnectionInfo
{
    public string ConnectionId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public string UserAgent { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public List<string> Groups { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public bool IsActive => (DateTime.UtcNow - LastActivity).TotalMinutes < 5;
}

/// <summary>
/// Interfaz para el hub de comunicación en tiempo real
/// </summary>
public interface IRealtimeHub
{
    Task SendToUserAsync(string userId, string method, object data);
    Task SendToGroupAsync(string groupName, string method, object data);
    Task SendToAllAsync(string method, object data);
    Task AddToGroupAsync(string connectionId, string groupName);
    Task RemoveFromGroupAsync(string connectionId, string groupName);
    Task<List<ConnectionInfo>> GetActiveConnectionsAsync();
    Task<List<string>> GetConnectedUsersAsync();
    event EventHandler<UserConnectedEventArgs> UserConnected;
    event EventHandler<UserDisconnectedEventArgs> UserDisconnected;
    event EventHandler<MessageSentEventArgs> MessageSent;
}

// ============================================================================
// REALTIME HUB IMPLEMENTATION - IMPLEMENTACIÓN DEL HUB
// ============================================================================

/// <summary>
/// Implementación del hub de comunicación en tiempo real (simula SignalR)
/// </summary>
public class RealtimeHub : IRealtimeHub, IDisposable
{
    private readonly ConcurrentDictionary<string, ConnectionInfo> _connections = new();
    private readonly ConcurrentDictionary<string, List<string>> _userConnections = new();
    private readonly ConcurrentDictionary<string, List<string>> _groups = new();
    private readonly Timer _cleanupTimer;

    public event EventHandler<UserConnectedEventArgs> UserConnected;
    public event EventHandler<UserDisconnectedEventArgs> UserDisconnected;
    public event EventHandler<MessageSentEventArgs> MessageSent;

    public RealtimeHub()
    {
        // Timer para limpiar conexiones inactivas cada 30 segundos
        _cleanupTimer = new Timer(CleanupInactiveConnections, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        Console.WriteLine("🌐 RealtimeHub iniciado");
    }

    public async Task<string> ConnectUserAsync(string userId, string userAgent = null)
    {
        var connectionInfo = new ConnectionInfo
        {
            UserId = userId,
            UserAgent = userAgent ?? "Unknown"
        };

        _connections[connectionInfo.ConnectionId] = connectionInfo;

        if (!_userConnections.ContainsKey(userId))
        {
            _userConnections[userId] = new List<string>();
        }
        _userConnections[userId].Add(connectionInfo.ConnectionId);

        Console.WriteLine($"🔗 Usuario conectado: {userId} (Connection: {connectionInfo.ConnectionId[..8]}...)");

        UserConnected?.Invoke(this, new UserConnectedEventArgs
        {
            UserId = userId,
            ConnectionId = connectionInfo.ConnectionId,
            ConnectedAt = connectionInfo.ConnectedAt
        });

        // Simular delay de red
        await Task.Delay(50);
        return connectionInfo.ConnectionId;
    }

    public async Task DisconnectUserAsync(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            // Remover de grupos
            foreach (var group in connection.Groups.ToList())
            {
                await RemoveFromGroupAsync(connectionId, group);
            }

            // Remover de conexiones de usuario
            if (_userConnections.ContainsKey(connection.UserId))
            {
                _userConnections[connection.UserId].Remove(connectionId);
                if (!_userConnections[connection.UserId].Any())
                {
                    _userConnections.TryRemove(connection.UserId, out _);
                }
            }

            Console.WriteLine($"🔌 Usuario desconectado: {connection.UserId} (Connection: {connectionId[..8]}...)");

            UserDisconnected?.Invoke(this, new UserDisconnectedEventArgs
            {
                UserId = connection.UserId,
                ConnectionId = connectionId,
                DisconnectedAt = DateTime.UtcNow,
                Duration = DateTime.UtcNow - connection.ConnectedAt
            });
        }

        await Task.Delay(50);
    }

    public async Task SendToUserAsync(string userId, string method, object data)
    {
        if (_userConnections.TryGetValue(userId, out var connectionIds))
        {
            var activeCons = connectionIds.Where(id => _connections.ContainsKey(id) && _connections[id].IsActive).ToList();
            
            if (activeCons.Any())
            {
                Console.WriteLine($"📤 Enviando a usuario {userId}: {method} ({activeCons.Count} conexiones)");
                
                // Simular envío a todas las conexiones del usuario
                foreach (var connectionId in activeCons)
                {
                    var connection = _connections[connectionId];
                    connection.LastActivity = DateTime.UtcNow;
                    
                    // Simular delay de red
                    await Task.Delay(10);
                }

                MessageSent?.Invoke(this, new MessageSentEventArgs
                {
                    TargetType = "User",
                    Target = userId,
                    Method = method,
                    Data = data,
                    ConnectionCount = activeCons.Count
                });
            }
            else
            {
                Console.WriteLine($"⚠️ Usuario {userId} no tiene conexiones activas");
            }
        }
        else
        {
            Console.WriteLine($"⚠️ Usuario {userId} no encontrado");
        }
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        if (_groups.TryGetValue(groupName, out var connectionIds))
        {
            var activeConnections = connectionIds.Where(id => _connections.ContainsKey(id) && _connections[id].IsActive).ToList();
            
            if (activeConnections.Any())
            {
                Console.WriteLine($"📤 Enviando a grupo {groupName}: {method} ({activeConnections.Count} conexiones)");
                
                foreach (var connectionId in activeConnections)
                {
                    var connection = _connections[connectionId];
                    connection.LastActivity = DateTime.UtcNow;
                    await Task.Delay(5); // Simular delay
                }

                MessageSent?.Invoke(this, new MessageSentEventArgs
                {
                    TargetType = "Group",
                    Target = groupName,
                    Method = method,
                    Data = data,
                    ConnectionCount = activeConnections.Count
                });
            }
        }
        else
        {
            Console.WriteLine($"⚠️ Grupo {groupName} no encontrado");
        }
    }

    public async Task SendToAllAsync(string method, object data)
    {
        var activeConnections = _connections.Values.Where(c => c.IsActive).ToList();
        
        Console.WriteLine($"📡 Enviando a todos: {method} ({activeConnections.Count} conexiones)");
        
        foreach (var connection in activeConnections)
        {
            connection.LastActivity = DateTime.UtcNow;
            await Task.Delay(2); // Simular delay mínimo
        }

        MessageSent?.Invoke(this, new MessageSentEventArgs
        {
            TargetType = "All",
            Target = "All",
            Method = method,
            Data = data,
            ConnectionCount = activeConnections.Count
        });
    }

    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        if (_connections.TryGetValue(connectionId, out var connection))
        {
            if (!_groups.ContainsKey(groupName))
            {
                _groups[groupName] = new List<string>();
            }

            if (!_groups[groupName].Contains(connectionId))
            {
                _groups[groupName].Add(connectionId);
                connection.Groups.Add(groupName);
                
                Console.WriteLine($"➕ Usuario {connection.UserId} agregado al grupo {groupName}");
            }
        }

        await Task.Delay(10);
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        if (_connections.TryGetValue(connectionId, out var connection))
        {
            if (_groups.ContainsKey(groupName))
            {
                _groups[groupName].Remove(connectionId);
                connection.Groups.Remove(groupName);
                
                if (!_groups[groupName].Any())
                {
                    _groups.TryRemove(groupName, out _);
                }
                
                Console.WriteLine($"➖ Usuario {connection.UserId} removido del grupo {groupName}");
            }
        }

        await Task.Delay(10);
    }

    public Task<List<ConnectionInfo>> GetActiveConnectionsAsync()
    {
        var activeConnections = _connections.Values
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.LastActivity)
            .ToList();

        return Task.FromResult(activeConnections);
    }

    public Task<List<string>> GetConnectedUsersAsync()
    {
        var connectedUsers = _userConnections.Keys
            .Where(userId => _userConnections[userId].Any(id => _connections.ContainsKey(id) && _connections[id].IsActive))
            .ToList();

        return Task.FromResult(connectedUsers);
    }

    private void CleanupInactiveConnections(object state)
    {
        var inactiveConnections = _connections.Values
            .Where(c => !c.IsActive)
            .ToList();

        foreach (var connection in inactiveConnections)
        {
            _ = Task.Run(() => DisconnectUserAsync(connection.ConnectionId));
        }

        if (inactiveConnections.Any())
        {
            Console.WriteLine($"🧹 Limpieza: {inactiveConnections.Count} conexiones inactivas removidas");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        Console.WriteLine("🛑 RealtimeHub detenido");
    }
}

// ============================================================================
// EVENT ARGS - ARGUMENTOS DE EVENTOS
// ============================================================================

public class UserConnectedEventArgs : EventArgs
{
    public string UserId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime ConnectedAt { get; set; }
}

public class UserDisconnectedEventArgs : EventArgs
{
    public string UserId { get; set; }
    public string ConnectionId { get; set; }
    public DateTime DisconnectedAt { get; set; }
    public TimeSpan Duration { get; set; }
}

public class MessageSentEventArgs : EventArgs
{
    public string TargetType { get; set; } // User, Group, All
    public string Target { get; set; }
    public string Method { get; set; }
    public object Data { get; set; }
    public int ConnectionCount { get; set; }
}

// ============================================================================
// NOTIFICATION SERVICES - SERVICIOS DE NOTIFICACIÓN
// ============================================================================

/// <summary>
/// Servicio de notificaciones en tiempo real
/// </summary>
public class RealtimeNotificationService
{
    private readonly IRealtimeHub _hub;

    public RealtimeNotificationService(IRealtimeHub hub)
    {
        _hub = hub;
    }

    public async Task SendOrderStatusUpdateAsync(string userId, object orderData)
    {
        await _hub.SendToUserAsync(userId, "OrderStatusUpdated", orderData);
    }

    public async Task SendInventoryAlertAsync(object inventoryData)
    {
        await _hub.SendToGroupAsync("inventory-managers", "InventoryAlert", inventoryData);
    }

    public async Task SendSystemMaintenanceNoticeAsync(object maintenanceData)
    {
        await _hub.SendToAllAsync("SystemMaintenance", maintenanceData);
    }

    public async Task SendChatMessageAsync(string roomId, object messageData)
    {
        await _hub.SendToGroupAsync($"chat-{roomId}", "NewMessage", messageData);
    }

    public async Task SendLiveOrderCountAsync(int orderCount)
    {
        await _hub.SendToGroupAsync("dashboard-users", "LiveOrderCount", new { Count = orderCount, Timestamp = DateTime.UtcNow });
    }
}

/// <summary>
/// Servicio de chat en tiempo real
/// </summary>
public class RealtimeChatService
{
    private readonly IRealtimeHub _hub;
    private readonly Dictionary<string, ChatRoom> _chatRooms = new();

    public RealtimeChatService(IRealtimeHub hub)
    {
        _hub = hub;
    }

    public async Task<string> CreateChatRoomAsync(string roomName, string createdBy)
    {
        var roomId = Guid.NewGuid().ToString();
        _chatRooms[roomId] = new ChatRoom
        {
            Id = roomId,
            Name = roomName,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        Console.WriteLine($"💬 Sala de chat creada: {roomName} (ID: {roomId[..8]}...)");
        return roomId;
    }

    public async Task JoinChatRoomAsync(string connectionId, string roomId, string userId)
    {
        if (_chatRooms.ContainsKey(roomId))
        {
            await _hub.AddToGroupAsync(connectionId, $"chat-{roomId}");
            _chatRooms[roomId].Participants.Add(userId);

            await _hub.SendToGroupAsync($"chat-{roomId}", "UserJoined", new
            {
                UserId = userId,
                RoomId = roomId,
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"👥 Usuario {userId} se unió a la sala {_chatRooms[roomId].Name}");
        }
    }

    public async Task LeaveChatRoomAsync(string connectionId, string roomId, string userId)
    {
        if (_chatRooms.ContainsKey(roomId))
        {
            await _hub.RemoveFromGroupAsync(connectionId, $"chat-{roomId}");
            _chatRooms[roomId].Participants.Remove(userId);

            await _hub.SendToGroupAsync($"chat-{roomId}", "UserLeft", new
            {
                UserId = userId,
                RoomId = roomId,
                Timestamp = DateTime.UtcNow
            });

            Console.WriteLine($"👋 Usuario {userId} salió de la sala {_chatRooms[roomId].Name}");
        }
    }

    public async Task SendMessageAsync(string roomId, string userId, string message)
    {
        if (_chatRooms.ContainsKey(roomId))
        {
            var messageData = new
            {
                Id = Guid.NewGuid().ToString(),
                RoomId = roomId,
                UserId = userId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            _chatRooms[roomId].Messages.Add(messageData);

            await _hub.SendToGroupAsync($"chat-{roomId}", "NewMessage", messageData);
            Console.WriteLine($"💬 Mensaje en {_chatRooms[roomId].Name}: {userId}: {message}");
        }
    }
}

/// <summary>
/// Servicio de actualizaciones de dashboard en tiempo real
/// </summary>
public class RealtimeDashboardService
{
    private readonly IRealtimeHub _hub;
    private readonly Timer _metricsTimer;
    private readonly Random _random = new();

    public RealtimeDashboardService(IRealtimeHub hub)
    {
        _hub = hub;
        
        // Timer para enviar métricas cada 5 segundos
        _metricsTimer = new Timer(SendLiveMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    public async Task SubscribeToDashboardAsync(string connectionId, string userId)
    {
        await _hub.AddToGroupAsync(connectionId, "dashboard-users");
        Console.WriteLine($"📊 Usuario {userId} suscrito al dashboard");

        // Enviar datos iniciales
        await SendInitialDashboardDataAsync(userId);
    }

    public async Task UnsubscribeFromDashboardAsync(string connectionId, string userId)
    {
        await _hub.RemoveFromGroupAsync(connectionId, "dashboard-users");
        Console.WriteLine($"📊 Usuario {userId} desuscrito del dashboard");
    }

    private async Task SendInitialDashboardDataAsync(string userId)
    {
        var initialData = new
        {
            TotalOrders = _random.Next(1000, 5000),
            TotalRevenue = _random.Next(50000, 200000),
            ActiveUsers = _random.Next(100, 1000),
            ConversionRate = Math.Round(_random.NextDouble() * 10, 2),
            LastUpdated = DateTime.UtcNow
        };

        await _hub.SendToUserAsync(userId, "InitialDashboardData", initialData);
    }

    private async void SendLiveMetrics(object state)
    {
        var metrics = new
        {
            OrdersPerMinute = _random.Next(5, 50),
            RevenuePerMinute = _random.Next(1000, 10000),
            ActiveUsers = _random.Next(100, 1000),
            ServerCpuUsage = Math.Round(_random.NextDouble() * 100, 1),
            ServerMemoryUsage = Math.Round(_random.NextDouble() * 100, 1),
            ResponseTime = _random.Next(50, 500),
            Timestamp = DateTime.UtcNow
        };

        await _hub.SendToGroupAsync("dashboard-users", "LiveMetrics", metrics);
    }

    public void Dispose()
    {
        _metricsTimer?.Dispose();
    }
}

// ============================================================================
// SUPPORTING MODELS - MODELOS DE APOYO
// ============================================================================

public class ChatRoom
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Participants { get; set; } = new();
    public List<object> Messages { get; set; } = new();
}

// ============================================================================
// DEMO REALISTA DE SIGNALR
// ============================================================================

public static class SignalRDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🎯 DEMO: SIGNALR PATTERN");
        Console.WriteLine("=========================");
        Console.WriteLine("Demostración de comunicación en tiempo real\n");

        using var hub = new RealtimeHub();
        var notificationService = new RealtimeNotificationService(hub);
        var chatService = new RealtimeChatService(hub);
        var dashboardService = new RealtimeDashboardService(hub);

        // Configurar event handlers
        hub.UserConnected += (sender, e) =>
        {
            Console.WriteLine($"🔔 Evento: Usuario {e.UserId} conectado a las {e.ConnectedAt:HH:mm:ss}");
        };

        hub.UserDisconnected += (sender, e) =>
        {
            Console.WriteLine($"🔔 Evento: Usuario {e.UserId} desconectado después de {e.Duration.TotalMinutes:F1} minutos");
        };

        hub.MessageSent += (sender, e) =>
        {
            Console.WriteLine($"📊 Estadística: Mensaje {e.Method} enviado a {e.TargetType} '{e.Target}' ({e.ConnectionCount} destinatarios)");
        };

        Console.WriteLine("👥 SIMULANDO CONEXIONES DE USUARIOS:");
        Console.WriteLine("====================================");

        // Simular conexiones de usuarios
        var users = new[]
        {
            ("admin", "Mozilla/5.0 Admin"),
            ("customer1", "Mobile App iOS"),
            ("customer2", "Chrome Browser"),
            ("support", "Support Dashboard"),
            ("manager", "Manager Portal")
        };

        var connectionIds = new Dictionary<string, string>();

        foreach (var (userId, userAgent) in users)
        {
            var connectionId = await hub.ConnectUserAsync(userId, userAgent);
            connectionIds[userId] = connectionId;
            await Task.Delay(500);
        }

        Console.WriteLine("\n💬 CONFIGURANDO CHAT:");
        Console.WriteLine("=====================");

        // Crear sala de chat
        var chatRoomId = await chatService.CreateChatRoomAsync("Soporte General", "admin");

        // Usuarios se unen al chat
        await chatService.JoinChatRoomAsync(connectionIds["admin"], chatRoomId, "admin");
        await chatService.JoinChatRoomAsync(connectionIds["customer1"], chatRoomId, "customer1");
        await chatService.JoinChatRoomAsync(connectionIds["support"], chatRoomId, "support");

        // Intercambio de mensajes
        await chatService.SendMessageAsync(chatRoomId, "customer1", "Hola, tengo un problema con mi pedido");
        await Task.Delay(1000);
        await chatService.SendMessageAsync(chatRoomId, "support", "¡Hola! Estaré encantado de ayudarte. ¿Cuál es tu número de pedido?");
        await Task.Delay(1000);
        await chatService.SendMessageAsync(chatRoomId, "customer1", "Es el pedido #12345");

        Console.WriteLine("\n📊 CONFIGURANDO DASHBOARD:");
        Console.WriteLine("==========================");

        // Suscribir usuarios al dashboard
        await dashboardService.SubscribeToDashboardAsync(connectionIds["admin"], "admin");
        await dashboardService.SubscribeToDashboardAsync(connectionIds["manager"], "manager");

        Console.WriteLine("\n📢 ENVIANDO NOTIFICACIONES:");
        Console.WriteLine("============================");

        // Notificaciones individuales
        await notificationService.SendOrderStatusUpdateAsync("customer1", new
        {
            OrderId = "12345",
            Status = "En tránsito",
            TrackingNumber = "TRK-987654321",
            EstimatedDelivery = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd")
        });

        await Task.Delay(1000);

        await notificationService.SendOrderStatusUpdateAsync("customer2", new
        {
            OrderId = "12346",
            Status = "Entregado",
            DeliveredAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        });

        // Crear grupo de inventario y agregar usuarios
        await hub.AddToGroupAsync(connectionIds["admin"], "inventory-managers");
        await hub.AddToGroupAsync(connectionIds["manager"], "inventory-managers");

        await Task.Delay(1000);

        // Alerta de inventario
        await notificationService.SendInventoryAlertAsync(new
        {
            ProductId = "PROD-123",
            ProductName = "Laptop Gaming",
            CurrentStock = 5,
            MinimumStock = 10,
            AlertLevel = "Low Stock"
        });

        await Task.Delay(2000);

        // Notificación global de mantenimiento
        await notificationService.SendSystemMaintenanceNoticeAsync(new
        {
            Title = "Mantenimiento Programado",
            Message = "El sistema estará en mantenimiento el domingo de 2:00 AM a 6:00 AM",
            ScheduledFor = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss"),
            Duration = "4 horas"
        });

        Console.WriteLine("\n📈 MONITOREANDO ACTIVIDAD (15 segundos):");
        Console.WriteLine("=========================================");

        // Simular actividad por 15 segundos
        var monitoringTask = MonitorConnectionsAsync(hub);
        await Task.Delay(15000);

        Console.WriteLine("\n🔌 SIMULANDO DESCONEXIONES:");
        Console.WriteLine("============================");

        // Algunos usuarios se desconectan
        await hub.DisconnectUserAsync(connectionIds["customer2"]);
        await Task.Delay(1000);
        await hub.DisconnectUserAsync(connectionIds["support"]);

        Console.WriteLine("\n📊 ESTADÍSTICAS FINALES:");
        Console.WriteLine("=========================");

        var activeConnections = await hub.GetActiveConnectionsAsync();
        var connectedUsers = await hub.GetConnectedUsersAsync();

        Console.WriteLine($"Conexiones activas: {activeConnections.Count}");
        Console.WriteLine($"Usuarios conectados: {connectedUsers.Count}");

        foreach (var user in connectedUsers)
        {
            var userConnections = activeConnections.Where(c => c.UserId == user).ToList();
            Console.WriteLine($"  • {user}: {userConnections.Count} conexiones");
        }

        Console.WriteLine("\n💡 BENEFICIOS DEL SIGNALR PATTERN:");
        Console.WriteLine("  • Comunicación bidireccional en tiempo real");
        Console.WriteLine("  • Gestión automática de conexiones y reconexiones");
        Console.WriteLine("  • Escalabilidad con grupos y canales");
        Console.WriteLine("  • Soporte para múltiples transportes (WebSockets, SSE, Long Polling)");
        Console.WriteLine("  • Integración fácil con aplicaciones web y móviles");
        Console.WriteLine("  • Notificaciones push instantáneas");

        // Cleanup
        dashboardService.Dispose();
    }

    private static async Task MonitorConnectionsAsync(IRealtimeHub hub)
    {
        for (int i = 0; i < 3; i++)
        {
            await Task.Delay(5000);
            
            var activeConnections = await hub.GetActiveConnectionsAsync();
            var connectedUsers = await hub.GetConnectedUsersAsync();
            
            Console.WriteLine($"📊 Monitor: {activeConnections.Count} conexiones activas, {connectedUsers.Count} usuarios conectados");
        }
    }
}
