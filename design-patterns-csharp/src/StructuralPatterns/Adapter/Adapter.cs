using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml;

// ============================================================================
// ADAPTER PATTERN - IMPLEMENTACIÓN DIDÁCTICA
// Ejemplo: Sistema de integración de APIs de diferentes proveedores de pago
// ============================================================================

// ============================================================================
// INTERFAZ OBJETIVO (TARGET) - Lo que nuestro sistema espera
// ============================================================================
public interface IPaymentProcessor
{
    PaymentResult ProcessPayment(PaymentRequest request);
    string GetSupportedCurrencies();
    bool IsServiceAvailable();
}

// Modelos de datos comunes para nuestro sistema
public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string CardNumber { get; set; }
    public string CustomerEmail { get; set; }
    public string Description { get; set; }
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string TransactionId { get; set; }
    public string Message { get; set; }
    public decimal ProcessedAmount { get; set; }
    public DateTime ProcessedAt { get; set; }
}

// ============================================================================
// SERVICIOS EXTERNOS INCOMPATIBLES (ADAPTEES)
// Cada uno tiene su propia interfaz y formato de datos
// ============================================================================

// Sistema PayPal (formato JSON, métodos específicos)
public class PayPalService
{
    public string ExecutePayPalTransaction(string jsonPayload)
    {
        Console.WriteLine("🔵 PayPal: Procesando transacción...");
        
        // Simular procesamiento de PayPal
        var paymentData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonPayload);
        var amount = decimal.Parse(paymentData["amount"].ToString());
        
        Console.WriteLine($"   💳 PayPal procesando ${amount}");
        
        var response = new
        {
            status = "completed",
            paypal_transaction_id = $"PP_{Guid.NewGuid().ToString()[..8]}",
            message = "Payment processed successfully via PayPal",
            processed_amount = amount,
            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        };
        
        return JsonSerializer.Serialize(response);
    }

    public string GetPayPalCurrencies()
    {
        return "USD,EUR,GBP,CAD,AUD";
    }

    public bool CheckPayPalStatus()
    {
        Console.WriteLine("🔵 PayPal: Verificando estado del servicio...");
        return true; // Simular servicio disponible
    }
}

// Sistema Stripe (diferentes métodos y estructura)
public class StripeApi
{
    public StripePaymentResponse ChargeCard(StripeChargeRequest request)
    {
        Console.WriteLine("🟢 Stripe: Ejecutando cargo a tarjeta...");
        Console.WriteLine($"   💳 Stripe procesando ${request.AmountInCents / 100m}");
        
        return new StripePaymentResponse
        {
            Id = $"ch_{Guid.NewGuid().ToString().Replace("-", "")[..16]}",
            Status = "succeeded",
            AmountInCents = request.AmountInCents,
            Currency = request.Currency,
            Description = request.Description,
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public List<string> GetSupportedCurrencies()
    {
        return new List<string> { "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF" };
    }

    public StripeStatusResponse GetServiceStatus()
    {
        Console.WriteLine("🟢 Stripe: Consultando estado del servicio...");
        return new StripeStatusResponse { IsOperational = true, LastUpdated = DateTime.UtcNow };
    }
}

// Modelos específicos de Stripe
public class StripeChargeRequest
{
    public long AmountInCents { get; set; }
    public string Currency { get; set; }
    public string Source { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class StripePaymentResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public long AmountInCents { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public long Created { get; set; }
}

public class StripeStatusResponse
{
    public bool IsOperational { get; set; }
    public DateTime LastUpdated { get; set; }
}

// Sistema bancario legacy (XML, protocolo antiguo)
public class LegacyBankSystem
{
    public string ProcessBankTransfer(string xmlRequest)
    {
        Console.WriteLine("🏦 Banco Legacy: Procesando transferencia bancaria...");
        
        var doc = new XmlDocument();
        doc.LoadXml(xmlRequest);
        
        var amountNode = doc.SelectSingleNode("//Amount");
        var amount = decimal.Parse(amountNode.InnerText);
        
        Console.WriteLine($"   💳 Banco Legacy procesando ${amount}");
        
        var responseXml = $@"
            <BankResponse>
                <Status>SUCCESS</Status>
                <TransactionId>BANK_{Guid.NewGuid().ToString()[..8]}</TransactionId>
                <Message>Transfer completed successfully</Message>
                <ProcessedAmount>{amount}</ProcessedAmount>
                <ProcessedDate>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</ProcessedDate>
            </BankResponse>";
        
        return responseXml;
    }

    public string GetAvailableCurrencies()
    {
        return "USD|EUR|GBP"; // Formato diferente
    }

    public int CheckSystemStatus()
    {
        Console.WriteLine("🏦 Banco Legacy: Verificando sistema...");
        return 1; // 1 = disponible, 0 = no disponible
    }
}

// ============================================================================
// ADAPTADORES - HACEN COMPATIBLES LOS SERVICIOS EXTERNOS
// ============================================================================

// Adaptador para PayPal
public class PayPalAdapter : IPaymentProcessor
{
    private readonly PayPalService _payPalService;

    public PayPalAdapter(PayPalService payPalService)
    {
        _payPalService = payPalService;
        Console.WriteLine("🔧 PayPal Adapter inicializado");
    }

    public PaymentResult ProcessPayment(PaymentRequest request)
    {
        try
        {
            // Convertir nuestro formato al formato de PayPal
            var payPalPayload = JsonSerializer.Serialize(new
            {
                amount = request.Amount.ToString(),
                currency = request.Currency,
                card_number = request.CardNumber,
                customer_email = request.CustomerEmail,
                description = request.Description
            });

            // Llamar al servicio de PayPal
            var responseJson = _payPalService.ExecutePayPalTransaction(payPalPayload);
            
            // Convertir la respuesta de PayPal a nuestro formato
            var payPalResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);
            
            return new PaymentResult
            {
                IsSuccess = payPalResponse["status"].ToString() == "completed",
                TransactionId = payPalResponse["paypal_transaction_id"].ToString(),
                Message = payPalResponse["message"].ToString(),
                ProcessedAmount = decimal.Parse(payPalResponse["processed_amount"].ToString()),
                ProcessedAt = DateTime.Parse(payPalResponse["timestamp"].ToString())
            };
        }
        catch (Exception ex)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                Message = $"PayPal Error: {ex.Message}",
                ProcessedAt = DateTime.Now
            };
        }
    }

    public string GetSupportedCurrencies()
    {
        var currencies = _payPalService.GetPayPalCurrencies();
        return currencies.Replace(",", ", "); // Normalizar formato
    }

    public bool IsServiceAvailable()
    {
        return _payPalService.CheckPayPalStatus();
    }
}

// Adaptador para Stripe
public class StripeAdapter : IPaymentProcessor
{
    private readonly StripeApi _stripeApi;

    public StripeAdapter(StripeApi stripeApi)
    {
        _stripeApi = stripeApi;
        Console.WriteLine("🔧 Stripe Adapter inicializado");
    }

    public PaymentResult ProcessPayment(PaymentRequest request)
    {
        try
        {
            // Convertir nuestro formato al formato de Stripe
            var stripeRequest = new StripeChargeRequest
            {
                AmountInCents = (long)(request.Amount * 100), // Stripe usa centavos
                Currency = request.Currency.ToLower(), // Stripe usa minúsculas
                Source = request.CardNumber,
                Description = request.Description,
                Metadata = new Dictionary<string, string>
                {
                    { "customer_email", request.CustomerEmail }
                }
            };

            // Llamar al servicio de Stripe
            var stripeResponse = _stripeApi.ChargeCard(stripeRequest);
            
            // Convertir la respuesta de Stripe a nuestro formato
            return new PaymentResult
            {
                IsSuccess = stripeResponse.Status == "succeeded",
                TransactionId = stripeResponse.Id,
                Message = $"Stripe payment {stripeResponse.Status}",
                ProcessedAmount = stripeResponse.AmountInCents / 100m,
                ProcessedAt = DateTimeOffset.FromUnixTimeSeconds(stripeResponse.Created).DateTime
            };
        }
        catch (Exception ex)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                Message = $"Stripe Error: {ex.Message}",
                ProcessedAt = DateTime.Now
            };
        }
    }

    public string GetSupportedCurrencies()
    {
        var currencies = _stripeApi.GetSupportedCurrencies();
        return string.Join(", ", currencies);
    }

    public bool IsServiceAvailable()
    {
        var status = _stripeApi.GetServiceStatus();
        return status.IsOperational;
    }
}

// Adaptador para el sistema bancario legacy
public class LegacyBankAdapter : IPaymentProcessor
{
    private readonly LegacyBankSystem _bankSystem;

    public LegacyBankAdapter(LegacyBankSystem bankSystem)
    {
        _bankSystem = bankSystem;
        Console.WriteLine("🔧 Legacy Bank Adapter inicializado");
    }

    public PaymentResult ProcessPayment(PaymentRequest request)
    {
        try
        {
            // Convertir nuestro formato al XML que espera el banco legacy
            var xmlRequest = $@"
                <BankRequest>
                    <Amount>{request.Amount}</Amount>
                    <Currency>{request.Currency}</Currency>
                    <CardNumber>{request.CardNumber}</CardNumber>
                    <CustomerEmail>{request.CustomerEmail}</CustomerEmail>
                    <Description>{request.Description}</Description>
                </BankRequest>";

            // Llamar al sistema bancario legacy
            var responseXml = _bankSystem.ProcessBankTransfer(xmlRequest);
            
            // Parsear la respuesta XML y convertir a nuestro formato
            var doc = new XmlDocument();
            doc.LoadXml(responseXml);
            
            return new PaymentResult
            {
                IsSuccess = doc.SelectSingleNode("//Status")?.InnerText == "SUCCESS",
                TransactionId = doc.SelectSingleNode("//TransactionId")?.InnerText,
                Message = doc.SelectSingleNode("//Message")?.InnerText,
                ProcessedAmount = decimal.Parse(doc.SelectSingleNode("//ProcessedAmount")?.InnerText ?? "0"),
                ProcessedAt = DateTime.Parse(doc.SelectSingleNode("//ProcessedDate")?.InnerText ?? DateTime.Now.ToString())
            };
        }
        catch (Exception ex)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                Message = $"Legacy Bank Error: {ex.Message}",
                ProcessedAt = DateTime.Now
            };
        }
    }

    public string GetSupportedCurrencies()
    {
        var currencies = _bankSystem.GetAvailableCurrencies();
        return currencies.Replace("|", ", "); // Convertir formato
    }

    public bool IsServiceAvailable()
    {
        return _bankSystem.CheckSystemStatus() == 1;
    }
}

// ============================================================================
// GESTOR DE PAGOS - USA LA INTERFAZ COMÚN
// ============================================================================
public class PaymentManager
{
    private readonly List<IPaymentProcessor> _processors;

    public PaymentManager()
    {
        _processors = new List<IPaymentProcessor>();
    }

    public void AddPaymentProcessor(IPaymentProcessor processor)
    {
        _processors.Add(processor);
        Console.WriteLine($"✅ Procesador de pagos agregado al sistema");
    }

    public PaymentResult ProcessPaymentWithFallback(PaymentRequest request)
    {
        Console.WriteLine($"\n💳 PROCESANDO PAGO DE ${request.Amount} {request.Currency}");
        Console.WriteLine("====================================================");

        foreach (var processor in _processors)
        {
            if (processor.IsServiceAvailable())
            {
                Console.WriteLine($"🔄 Intentando procesar con {processor.GetType().Name}...");
                var result = processor.ProcessPayment(request);
                
                if (result.IsSuccess)
                {
                    Console.WriteLine($"✅ Pago procesado exitosamente!");
                    return result;
                }
                else
                {
                    Console.WriteLine($"❌ Falló: {result.Message}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ {processor.GetType().Name} no disponible, probando siguiente...");
            }
        }

        return new PaymentResult
        {
            IsSuccess = false,
            Message = "Todos los procesadores de pago fallaron",
            ProcessedAt = DateTime.Now
        };
    }

    public void ShowAvailableProcessors()
    {
        Console.WriteLine("\n🔍 PROCESADORES DE PAGO DISPONIBLES");
        Console.WriteLine("===================================");
        
        for (int i = 0; i < _processors.Count; i++)
        {
            var processor = _processors[i];
            var status = processor.IsServiceAvailable() ? "✅ Disponible" : "❌ No disponible";
            var currencies = processor.GetSupportedCurrencies();
            
            Console.WriteLine($"{i + 1}. {processor.GetType().Name}");
            Console.WriteLine($"   Estado: {status}");
            Console.WriteLine($"   Monedas: {currencies}");
        }
    }
}

// ============================================================================
// DEMO DIDÁCTICO
// ============================================================================
public static class AdapterPatternDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("🔌 DEMO: ADAPTER PATTERN - INTEGRACIÓN DE SISTEMAS DE PAGO");
        Console.WriteLine("============================================================");

        // Crear los servicios externos (incompatibles entre sí)
        var payPalService = new PayPalService();
        var stripeApi = new StripeApi();
        var legacyBank = new LegacyBankSystem();

        // Crear los adaptadores que los hacen compatibles
        var payPalAdapter = new PayPalAdapter(payPalService);
        var stripeAdapter = new StripeAdapter(stripeApi);
        var bankAdapter = new LegacyBankAdapter(legacyBank);

        // Crear el gestor que usa la interfaz común
        var paymentManager = new PaymentManager();
        paymentManager.AddPaymentProcessor(payPalAdapter);
        paymentManager.AddPaymentProcessor(stripeAdapter);
        paymentManager.AddPaymentProcessor(bankAdapter);

        // Mostrar procesadores disponibles
        paymentManager.ShowAvailableProcessors();

        // Ejemplos de pagos
        var payments = new[]
        {
            new PaymentRequest
            {
                Amount = 299.99m,
                Currency = "USD",
                CardNumber = "4111111111111111",
                CustomerEmail = "cliente1@ejemplo.com",
                Description = "Compra de laptop"
            },
            new PaymentRequest
            {
                Amount = 59.99m,
                Currency = "EUR",
                CardNumber = "5555555555554444",
                CustomerEmail = "cliente2@ejemplo.com",
                Description = "Suscripción mensual"
            },
            new PaymentRequest
            {
                Amount = 1299.99m,
                Currency = "GBP",
                CardNumber = "378282246310005",
                CustomerEmail = "cliente3@ejemplo.com",
                Description = "Equipamiento profesional"
            }
        };

        // Procesar cada pago
        foreach (var payment in payments)
        {
            var result = paymentManager.ProcessPaymentWithFallback(payment);
            
            Console.WriteLine("\n📊 RESULTADO DEL PAGO:");
            Console.WriteLine($"   ✅ Éxito: {result.IsSuccess}");
            Console.WriteLine($"   🆔 ID Transacción: {result.TransactionId}");
            Console.WriteLine($"   💰 Monto Procesado: ${result.ProcessedAmount}");
            Console.WriteLine($"   📅 Fecha: {result.ProcessedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"   💬 Mensaje: {result.Message}");
            
            Console.WriteLine("\n" + new string('-', 50));
        }

        Console.WriteLine("\n✅ Demo del Adapter Pattern completado");
        Console.WriteLine("\n💡 LECCIONES APRENDIDAS:");
        Console.WriteLine("   • El Adapter permite usar interfaces incompatibles de forma uniforme");
        Console.WriteLine("   • Cada adaptador convierte entre formatos específicos y el formato común");
        Console.WriteLine("   • El cliente (PaymentManager) no necesita saber sobre las diferencias");
        Console.WriteLine("   • Se puede agregar soporte para nuevos sistemas sin cambiar código existente");
        Console.WriteLine("   • Permite reutilizar código legacy sin modificarlo");
    }
}

// Para ejecutar el demo:
// AdapterPatternDemo.RunDemo();
