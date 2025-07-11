using System;

// ============================================================================
// FACTORY METHOD PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de notificaciones empresariales
// ============================================================================

// Producto abstracto
public interface INotification
{
    void Send(string recipient, string message);
    string GetDeliveryStatus();
    decimal GetCost();
}

// Productos concretos - Implementaciones específicas de notificaciones
public class EmailNotification : INotification
{
    private bool _sent = false;
    private DateTime _sentAt;

    public void Send(string recipient, string message)
    {
        // Simular envío de email
        Console.WriteLine($"📧 Enviando email a {recipient}: {message}");
        Console.WriteLine($"   → Configurando servidor SMTP...");
        Console.WriteLine($"   → Validando formato de email...");
        Console.WriteLine($"   → Email enviado exitosamente");
        _sent = true;
        _sentAt = DateTime.Now;
    }

    public string GetDeliveryStatus()
    {
        return _sent ? $"✅ Entregado vía Email el {_sentAt:HH:mm:ss}" : "❌ No enviado";
    }

    public decimal GetCost() => 0.01m; // $0.01 por email
}

public class SmsNotification : INotification
{
    private bool _sent = false;
    private DateTime _sentAt;

    public void Send(string recipient, string message)
    {
        // Simular envío de SMS
        Console.WriteLine($"📱 Enviando SMS a {recipient}: {message}");
        Console.WriteLine($"   → Conectando con proveedor de SMS...");
        Console.WriteLine($"   → Validando número de teléfono...");
        Console.WriteLine($"   → SMS enviado exitosamente");
        _sent = true;
        _sentAt = DateTime.Now;
    }

    public string GetDeliveryStatus()
    {
        return _sent ? $"✅ Entregado vía SMS el {_sentAt:HH:mm:ss}" : "❌ No enviado";
    }

    public decimal GetCost() => 0.05m; // $0.05 por SMS
}

public class PushNotification : INotification
{
    private bool _sent = false;
    private DateTime _sentAt;

    public void Send(string recipient, string message)
    {
        // Simular envío de push notification
        Console.WriteLine($"🔔 Enviando push notification a {recipient}: {message}");
        Console.WriteLine($"   → Conectando con servicio push...");
        Console.WriteLine($"   → Validando token de dispositivo...");
        Console.WriteLine($"   → Push notification enviada exitosamente");
        _sent = true;
        _sentAt = DateTime.Now;
    }

    public string GetDeliveryStatus()
    {
        return _sent ? $"✅ Entregado vía Push el {_sentAt:HH:mm:ss}" : "❌ No enviado";
    }

    public decimal GetCost() => 0.002m; // $0.002 por push
}

// Creator abstracto - Define el Factory Method
public abstract class NotificationCreator
{
    // Factory Method - debe ser implementado por subclases
    public abstract INotification CreateNotification();

    // Método de negocio que usa el Factory Method
    public void ProcessNotification(string recipient, string message)
    {
        Console.WriteLine($"\n🔄 Procesando notificación para {recipient}...");
        
        // Usar el Factory Method para crear la notificación apropiada
        INotification notification = CreateNotification();
        
        // Enviar la notificación
        notification.Send(recipient, message);
        
        // Mostrar estadísticas
        Console.WriteLine($"📊 Estado: {notification.GetDeliveryStatus()}");
        Console.WriteLine($"💰 Costo: ${notification.GetCost():F3}");
    }
}

// Creators concretos - Implementan el Factory Method
public class EmailNotificationCreator : NotificationCreator
{
    public override INotification CreateNotification()
    {
        return new EmailNotification();
    }
}

public class SmsNotificationCreator : NotificationCreator
{
    public override INotification CreateNotification()
    {
        return new SmsNotification();
    }
}

public class PushNotificationCreator : NotificationCreator
{
    public override INotification CreateNotification()
    {
        return new PushNotification();
    }
}

// ============================================================================
// EJEMPLO DE USO REALISTA
// ============================================================================
public static class NotificationDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("🏢 SISTEMA DE NOTIFICACIONES EMPRESARIALES");
        Console.WriteLine("==========================================");

        // Diferentes tipos de notificaciones según el contexto
        var scenarios = new[]
        {
            ("Alerta de seguridad", "juan@empresa.com", new EmailNotificationCreator()),
            ("Código de verificación", "+1-555-0123", new SmsNotificationCreator()),
            ("Recordatorio de reunión", "user_token_123", new PushNotificationCreator())
        };

        decimal totalCost = 0;

        foreach (var (messageType, recipient, creator) in scenarios)
        {
            Console.WriteLine($"\n📋 Escenario: {messageType}");
            creator.ProcessNotification(recipient, $"Mensaje: {messageType}");
            
            // Calcular costo acumulado
            var notification = creator.CreateNotification();
            totalCost += notification.GetCost();
        }

        Console.WriteLine($"\n💰 Costo total de notificaciones: ${totalCost:F3}");
        Console.WriteLine("✅ Proceso completado exitosamente");
    }
}

// ============================================================================
// USO AVANZADO: Factory Method con configuración
// ============================================================================
public class ConfigurableNotificationCreator : NotificationCreator
{
    private readonly NotificationType _type;
    private readonly string _apiKey;

    public enum NotificationType
    {
        Email,
        Sms,
        Push
    }

    public ConfigurableNotificationCreator(NotificationType type, string apiKey = "default-key")
    {
        _type = type;
        _apiKey = apiKey;
    }

    public override INotification CreateNotification()
    {
        Console.WriteLine($"🔧 Configurando notificación con API Key: {_apiKey[..8]}...");
        
        return _type switch
        {
            NotificationType.Email => new EmailNotification(),
            NotificationType.Sms => new SmsNotification(),
            NotificationType.Push => new PushNotification(),
            _ => throw new ArgumentException($"Tipo de notificación no soportado: {_type}")
        };
    }
}

// Para ejecutar el demo:
// NotificationDemo.RunDemo();
