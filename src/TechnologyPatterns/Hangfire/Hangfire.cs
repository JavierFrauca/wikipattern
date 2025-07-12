using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Text.Json;

// ============================================================================
// HANGFIRE PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: Sistema de procesamiento de tareas en background para e-commerce
// ============================================================================

// ============================================================================
// JOB INFRASTRUCTURE - INFRAESTRUCTURA DE TRABAJOS
// ============================================================================

/// <summary>
/// Estado de un trabajo
/// </summary>
public enum JobState
{
    Enqueued,
    Processing,
    Succeeded,
    Failed,
    Scheduled,
    Deleted
}

/// <summary>
/// Información de un trabajo
/// </summary>
public class JobInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobType { get; set; }
    public string MethodName { get; set; }
    public object[] Arguments { get; set; }
    public JobState State { get; set; } = JobState.Enqueued;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string Exception { get; set; }
    public int AttemptCount { get; set; } = 0;
    public string Queue { get; set; } = "default";
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// Resultado de ejecución de un trabajo
/// </summary>
public class JobResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
    public Exception Exception { get; set; }

    public static JobResult Success(string message = null, object data = null)
        => new() { IsSuccess = true, Message = message, Data = data };

    public static JobResult Failure(string message, Exception exception = null)
        => new() { IsSuccess = false, Message = message, Exception = exception };
}

/// <summary>
/// Interfaz para trabajos que se pueden ejecutar
/// </summary>
public interface IBackgroundJob
{
    Task<JobResult> ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz para el scheduler de trabajos
/// </summary>
public interface IJobScheduler
{
    string Enqueue<T>(string methodName, params object[] args) where T : class;
    string EnqueueIn<T>(TimeSpan delay, string methodName, params object[] args) where T : class;
    string Schedule<T>(DateTime scheduleAt, string methodName, params object[] args) where T : class;
    string AddRecurring<T>(string recurringJobId, string cronExpression, string methodName, params object[] args) where T : class;
    bool Delete(string jobId);
    JobInfo GetJob(string jobId);
    List<JobInfo> GetJobs(JobState? state = null, string queue = null);
}

// ============================================================================
// JOB SCHEDULER IMPLEMENTATION - IMPLEMENTACIÓN DEL SCHEDULER
// ============================================================================

/// <summary>
/// Implementación simple del scheduler de trabajos (simula Hangfire)
/// </summary>
public class BackgroundJobScheduler : IJobScheduler, IDisposable
{
    private readonly ConcurrentDictionary<string, JobInfo> _jobs = new();
    private readonly ConcurrentDictionary<string, RecurringJobInfo> _recurringJobs = new();
    private readonly Dictionary<string, ConcurrentQueue<JobInfo>> _queues = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<Task> _workers = new();
    private readonly object _lock = new();

    public event EventHandler<JobExecutedEventArgs> JobExecuted;
    public event EventHandler<JobFailedEventArgs> JobFailed;
    public event EventHandler<JobScheduledEventArgs> JobScheduled;

    public BackgroundJobScheduler(int workerCount = 3)
    {
        // Inicializar colas
        _queues["default"] = new ConcurrentQueue<JobInfo>();
        _queues["critical"] = new ConcurrentQueue<JobInfo>();
        _queues["email"] = new ConcurrentQueue<JobInfo>();
        _queues["reports"] = new ConcurrentQueue<JobInfo>();

        // Iniciar workers
        for (int i = 0; i < workerCount; i++)
        {
            var workerId = i + 1;
            _workers.Add(Task.Run(() => ProcessJobsAsync(workerId, _cancellationTokenSource.Token)));
        }

        // Iniciar scheduler para trabajos programados
        _workers.Add(Task.Run(() => ProcessScheduledJobsAsync(_cancellationTokenSource.Token)));
        _workers.Add(Task.Run(() => ProcessRecurringJobsAsync(_cancellationTokenSource.Token)));

        Console.WriteLine($"🚀 BackgroundJobScheduler iniciado con {workerCount} workers");
    }

    public string Enqueue<T>(string methodName, params object[] args) where T : class
    {
        var job = new JobInfo
        {
            JobType = typeof(T).Name,
            MethodName = methodName,
            Arguments = args,
            State = JobState.Enqueued,
            Queue = "default"
        };

        _jobs[job.Id] = job;
        _queues[job.Queue].Enqueue(job);

        Console.WriteLine($"📋 Job encolado: {job.JobType}.{job.MethodName} (ID: {job.Id[..8]}...)");
        
        JobScheduled?.Invoke(this, new JobScheduledEventArgs { JobInfo = job });
        return job.Id;
    }

    public string EnqueueIn<T>(TimeSpan delay, string methodName, params object[] args) where T : class
    {
        return Schedule<T>(DateTime.UtcNow.Add(delay), methodName, args);
    }

    public string Schedule<T>(DateTime scheduleAt, string methodName, params object[] args) where T : class
    {
        var job = new JobInfo
        {
            JobType = typeof(T).Name,
            MethodName = methodName,
            Arguments = args,
            State = JobState.Scheduled,
            ScheduledAt = scheduleAt,
            Queue = "default"
        };

        _jobs[job.Id] = job;

        Console.WriteLine($"⏰ Job programado: {job.JobType}.{job.MethodName} para {scheduleAt:yyyy-MM-dd HH:mm:ss}");
        
        JobScheduled?.Invoke(this, new JobScheduledEventArgs { JobInfo = job });
        return job.Id;
    }

    public string AddRecurring<T>(string recurringJobId, string cronExpression, string methodName, params object[] args) where T : class
    {
        var recurringJob = new RecurringJobInfo
        {
            Id = recurringJobId,
            JobType = typeof(T).Name,
            MethodName = methodName,
            Arguments = args,
            CronExpression = cronExpression,
            NextRun = CalculateNextRun(cronExpression)
        };

        _recurringJobs[recurringJobId] = recurringJob;

        Console.WriteLine($"🔄 Job recurrente agregado: {recurringJobId} - {cronExpression}");
        return recurringJobId;
    }

    public bool Delete(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.State = JobState.Deleted;
            Console.WriteLine($"🗑️ Job eliminado: {jobId[..8]}...");
            return true;
        }
        return false;
    }

    public JobInfo GetJob(string jobId)
    {
        return _jobs.TryGetValue(jobId, out var job) ? job : null;
    }

    public List<JobInfo> GetJobs(JobState? state = null, string queue = null)
    {
        var jobs = _jobs.Values.AsEnumerable();

        if (state.HasValue)
            jobs = jobs.Where(j => j.State == state.Value);

        if (!string.IsNullOrEmpty(queue))
            jobs = jobs.Where(j => j.Queue == queue);

        return jobs.OrderByDescending(j => j.CreatedAt).ToList();
    }

    private async Task ProcessJobsAsync(int workerId, CancellationToken cancellationToken)
    {
        Console.WriteLine($"👷 Worker {workerId} iniciado");

        while (!cancellationToken.IsCancellationRequested)
        {
            JobInfo job = null;
            bool foundJob = false;

            // Buscar trabajo en orden de prioridad: critical, default, email, reports
            foreach (var queueName in new[] { "critical", "default", "email", "reports" })
            {
                if (_queues[queueName].TryDequeue(out job))
                {
                    foundJob = true;
                    break;
                }
            }

            if (!foundJob)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            if (job.State == JobState.Deleted)
                continue;

            await ExecuteJobAsync(workerId, job, cancellationToken);
        }

        Console.WriteLine($"👷 Worker {workerId} detenido");
    }

    private async Task ExecuteJobAsync(int workerId, JobInfo job, CancellationToken cancellationToken)
    {
        job.State = JobState.Processing;
        job.ProcessedAt = DateTime.UtcNow;
        job.AttemptCount++;

        Console.WriteLine($"⚡ Worker {workerId} ejecutando: {job.JobType}.{job.MethodName} (Intento {job.AttemptCount})");

        try
        {
            var result = await InvokeJobMethodAsync(job, cancellationToken);

            if (result.IsSuccess)
            {
                job.State = JobState.Succeeded;
                Console.WriteLine($"✅ Job completado: {job.JobType}.{job.MethodName} - {result.Message}");
                
                JobExecuted?.Invoke(this, new JobExecutedEventArgs 
                { 
                    JobInfo = job, 
                    Result = result,
                    WorkerId = workerId
                });
            }
            else
            {
                HandleJobFailure(job, result.Exception ?? new Exception(result.Message));
            }
        }
        catch (Exception ex)
        {
            HandleJobFailure(job, ex);
        }
    }

    private void HandleJobFailure(JobInfo job, Exception ex)
    {
        job.Exception = ex.Message;

        if (job.AttemptCount < 3) // Máximo 3 intentos
        {
            job.State = JobState.Enqueued;
            job.ScheduledAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, job.AttemptCount)); // Backoff exponencial
            
            Console.WriteLine($"🔄 Job reintentará en {job.ScheduledAt}: {job.JobType}.{job.MethodName}");
        }
        else
        {
            job.State = JobState.Failed;
            Console.WriteLine($"❌ Job falló definitivamente: {job.JobType}.{job.MethodName} - {ex.Message}");
            
            JobFailed?.Invoke(this, new JobFailedEventArgs 
            { 
                JobInfo = job, 
                Exception = ex 
            });
        }
    }

    private async Task<JobResult> InvokeJobMethodAsync(JobInfo job, CancellationToken cancellationToken)
    {
        // Simular diferentes tipos de trabajos
        return job.JobType switch
        {
            "EmailService" => await ExecuteEmailJob(job, cancellationToken),
            "ReportService" => await ExecuteReportJob(job, cancellationToken),
            "InventoryService" => await ExecuteInventoryJob(job, cancellationToken),
            "NotificationService" => await ExecuteNotificationJob(job, cancellationToken),
            "OrderService" => await ExecuteOrderJob(job, cancellationToken),
            _ => JobResult.Failure($"Tipo de trabajo desconocido: {job.JobType}")
        };
    }

    private async Task<JobResult> ExecuteEmailJob(JobInfo job, CancellationToken cancellationToken)
    {
        await Task.Delay(2000, cancellationToken); // Simular envío de email

        if (job.MethodName == "SendWelcomeEmail")
        {
            var email = job.Arguments[0].ToString();
            return JobResult.Success($"Email de bienvenida enviado a {email}");
        }
        else if (job.MethodName == "SendOrderConfirmation")
        {
            var orderId = job.Arguments[0].ToString();
            var email = job.Arguments[1].ToString();
            return JobResult.Success($"Confirmación de orden {orderId} enviada a {email}");
        }

        return JobResult.Failure("Método de email no reconocido");
    }

    private async Task<JobResult> ExecuteReportJob(JobInfo job, CancellationToken cancellationToken)
    {
        await Task.Delay(5000, cancellationToken); // Simular generación de reporte

        if (job.MethodName == "GenerateMonthlyReport")
        {
            var month = job.Arguments[0].ToString();
            return JobResult.Success($"Reporte mensual generado para {month}");
        }
        else if (job.MethodName == "GenerateSalesReport")
        {
            var fromDate = job.Arguments[0].ToString();
            var toDate = job.Arguments[1].ToString();
            return JobResult.Success($"Reporte de ventas generado del {fromDate} al {toDate}");
        }

        return JobResult.Failure("Método de reporte no reconocido");
    }

    private async Task<JobResult> ExecuteInventoryJob(JobInfo job, CancellationToken cancellationToken)
    {
        await Task.Delay(1500, cancellationToken);

        if (job.MethodName == "UpdateStock")
        {
            var productId = job.Arguments[0].ToString();
            var quantity = job.Arguments[1].ToString();
            return JobResult.Success($"Stock actualizado para producto {productId}: {quantity} unidades");
        }
        else if (job.MethodName == "CheckLowStock")
        {
            return JobResult.Success("Revisión de stock bajo completada");
        }

        return JobResult.Failure("Método de inventario no reconocido");
    }

    private async Task<JobResult> ExecuteNotificationJob(JobInfo job, CancellationToken cancellationToken)
    {
        await Task.Delay(800, cancellationToken);

        if (job.MethodName == "SendPushNotification")
        {
            var userId = job.Arguments[0].ToString();
            var message = job.Arguments[1].ToString();
            return JobResult.Success($"Push notification enviada a usuario {userId}: {message}");
        }

        return JobResult.Failure("Método de notificación no reconocido");
    }

    private async Task<JobResult> ExecuteOrderJob(JobInfo job, CancellationToken cancellationToken)
    {
        await Task.Delay(3000, cancellationToken);

        if (job.MethodName == "ProcessRefund")
        {
            var orderId = job.Arguments[0].ToString();
            var amount = job.Arguments[1].ToString();
            
            // Simular fallo ocasional
            if (new Random().Next(1, 10) <= 2) // 20% de fallo
            {
                return JobResult.Failure("Error en procesamiento de refund - banco no disponible");
            }
            
            return JobResult.Success($"Refund procesado para orden {orderId}: ${amount}");
        }

        return JobResult.Failure("Método de orden no reconocido");
    }

    private async Task ProcessScheduledJobsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var scheduledJobs = _jobs.Values
                .Where(j => j.State == JobState.Scheduled && j.ScheduledAt <= now)
                .ToList();

            foreach (var job in scheduledJobs)
            {
                job.State = JobState.Enqueued;
                _queues[job.Queue].Enqueue(job);
                Console.WriteLine($"⏰ Job programado movido a cola: {job.JobType}.{job.MethodName}");
            }

            await Task.Delay(5000, cancellationToken); // Revisar cada 5 segundos
        }
    }

    private async Task ProcessRecurringJobsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            
            foreach (var recurringJob in _recurringJobs.Values.ToList())
            {
                if (recurringJob.NextRun <= now)
                {
                    var job = new JobInfo
                    {
                        JobType = recurringJob.JobType,
                        MethodName = recurringJob.MethodName,
                        Arguments = recurringJob.Arguments,
                        State = JobState.Enqueued,
                        Queue = "default"
                    };

                    _jobs[job.Id] = job;
                    _queues[job.Queue].Enqueue(job);

                    recurringJob.NextRun = CalculateNextRun(recurringJob.CronExpression);
                    Console.WriteLine($"🔄 Job recurrente ejecutado: {recurringJob.Id}, próximo: {recurringJob.NextRun}");
                }
            }

            await Task.Delay(10000, cancellationToken); // Revisar cada 10 segundos
        }
    }

    private DateTime CalculateNextRun(string cronExpression)
    {
        // Implementación simplificada de CRON
        return cronExpression switch
        {
            "*/5 * * * *" => DateTime.UtcNow.AddMinutes(5), // Cada 5 minutos
            "0 * * * *" => DateTime.UtcNow.AddHours(1),     // Cada hora
            "0 0 * * *" => DateTime.UtcNow.AddDays(1),      // Diario
            "0 0 * * 0" => DateTime.UtcNow.AddDays(7),      // Semanal
            _ => DateTime.UtcNow.AddMinutes(10)             // Default: 10 minutos
        };
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        Task.WaitAll(_workers.ToArray(), TimeSpan.FromSeconds(10));
        _cancellationTokenSource.Dispose();
        Console.WriteLine("🛑 BackgroundJobScheduler detenido");
    }
}

// ============================================================================
// SUPPORTING CLASSES - CLASES DE APOYO
// ============================================================================

public class RecurringJobInfo
{
    public string Id { get; set; }
    public string JobType { get; set; }
    public string MethodName { get; set; }
    public object[] Arguments { get; set; }
    public string CronExpression { get; set; }
    public DateTime NextRun { get; set; }
    public DateTime LastRun { get; set; }
}

public class JobExecutedEventArgs : EventArgs
{
    public JobInfo JobInfo { get; set; }
    public JobResult Result { get; set; }
    public int WorkerId { get; set; }
}

public class JobFailedEventArgs : EventArgs
{
    public JobInfo JobInfo { get; set; }
    public Exception Exception { get; set; }
}

public class JobScheduledEventArgs : EventArgs
{
    public JobInfo JobInfo { get; set; }
}

// ============================================================================
// SERVICE IMPLEMENTATIONS - SERVICIOS DE EJEMPLO
// ============================================================================

public class EmailService
{
    private readonly IJobScheduler _jobScheduler;

    public EmailService(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    public string SendWelcomeEmailAsync(string email)
    {
        return _jobScheduler.Enqueue<EmailService>("SendWelcomeEmail", email);
    }

    public string SendOrderConfirmationAsync(string orderId, string email)
    {
        return _jobScheduler.Enqueue<EmailService>("SendOrderConfirmation", orderId, email);
    }

    public string ScheduleReminderEmailAsync(string email, DateTime sendAt)
    {
        return _jobScheduler.Schedule<EmailService>(sendAt, "SendReminderEmail", email);
    }
}

public class ReportService
{
    private readonly IJobScheduler _jobScheduler;

    public ReportService(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    public string GenerateMonthlyReportAsync(string month)
    {
        return _jobScheduler.Enqueue<ReportService>("GenerateMonthlyReport", month);
    }

    public string GenerateSalesReportAsync(DateTime fromDate, DateTime toDate)
    {
        return _jobScheduler.Enqueue<ReportService>("GenerateSalesReport", 
            fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"));
    }

    public void SetupRecurringReports()
    {
        _jobScheduler.AddRecurring<ReportService>("monthly-sales", "0 0 1 * *", "GenerateMonthlyReport", DateTime.Now.ToString("yyyy-MM"));
        _jobScheduler.AddRecurring<ReportService>("daily-inventory", "0 2 * * *", "GenerateInventoryReport");
    }
}

public class OrderService
{
    private readonly IJobScheduler _jobScheduler;

    public OrderService(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    public string ProcessRefundAsync(string orderId, decimal amount)
    {
        return _jobScheduler.Enqueue<OrderService>("ProcessRefund", orderId, amount.ToString("F2"));
    }

    public string ScheduleOrderReminderAsync(string orderId, TimeSpan delay)
    {
        return _jobScheduler.EnqueueIn<OrderService>(delay, "SendOrderReminder", orderId);
    }
}

// ============================================================================
// DEMO REALISTA DE HANGFIRE
// ============================================================================

public static class HangfireDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🎯 DEMO: HANGFIRE PATTERN");
        Console.WriteLine("==========================");
        Console.WriteLine("Demostración de procesamiento de tareas en background\n");

        using var scheduler = new BackgroundJobScheduler(workerCount: 3);

        // Configurar servicios
        var emailService = new EmailService(scheduler);
        var reportService = new ReportService(scheduler);
        var orderService = new OrderService(scheduler);

        Console.WriteLine("📧 PROGRAMANDO TRABAJOS DE EMAIL:");
        Console.WriteLine("==================================");

        var emailJobs = new List<string>
        {
            emailService.SendWelcomeEmailAsync("nuevo.usuario@example.com"),
            emailService.SendOrderConfirmationAsync("ORD-12345", "cliente@example.com"),
            emailService.SendOrderConfirmationAsync("ORD-12346", "cliente2@example.com"),
            emailService.ScheduleReminderEmailAsync("reminder@example.com", DateTime.UtcNow.AddSeconds(30))
        };

        Console.WriteLine($"✅ {emailJobs.Count} trabajos de email programados");

        Console.WriteLine("\n📊 PROGRAMANDO TRABAJOS DE REPORTES:");
        Console.WriteLine("====================================");

        var reportJobs = new List<string>
        {
            reportService.GenerateMonthlyReportAsync("2024-12"),
            reportService.GenerateSalesReportAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow)
        };

        // Configurar reportes recurrentes
        reportService.SetupRecurringReports();

        Console.WriteLine($"✅ {reportJobs.Count} trabajos de reportes programados");
        Console.WriteLine("✅ Reportes recurrentes configurados");

        Console.WriteLine("\n💰 PROGRAMANDO TRABAJOS DE REFUNDS:");
        Console.WriteLine("===================================");

        var refundJobs = new List<string>
        {
            orderService.ProcessRefundAsync("ORD-12340", 99.99m),
            orderService.ProcessRefundAsync("ORD-12341", 249.50m),
            orderService.ProcessRefundAsync("ORD-12342", 1299.99m) // Este podría fallar
        };

        Console.WriteLine($"✅ {refundJobs.Count} trabajos de refund programados");

        Console.WriteLine("\n⏰ PROGRAMANDO TRABAJOS DIFERIDOS:");
        Console.WriteLine("==================================");

        var delayedJobs = new List<string>
        {
            orderService.ScheduleOrderReminderAsync("ORD-12350", TimeSpan.FromSeconds(45)),
            scheduler.EnqueueIn<NotificationService>(TimeSpan.FromSeconds(60), "SendPushNotification", "USER-123", "Tu pedido está en camino"),
            scheduler.Schedule<InventoryService>(DateTime.UtcNow.AddSeconds(90), "CheckLowStock")
        };

        Console.WriteLine($"✅ {delayedJobs.Count} trabajos diferidos programados");

        // Monitorear progreso
        var monitoringTask = MonitorJobsAsync(scheduler);

        Console.WriteLine("\n🏃 PROCESANDO TRABAJOS...");
        Console.WriteLine("===========================");

        // Esperar a que se procesen algunos trabajos
        await Task.Delay(20000);

        Console.WriteLine("\n📊 ESTADÍSTICAS FINALES:");
        Console.WriteLine("=========================");

        var allJobs = scheduler.GetJobs();
        var stats = allJobs.GroupBy(j => j.State).ToDictionary(g => g.Key, g => g.Count());

        foreach (var stat in stats)
        {
            Console.WriteLine($"  {GetStateEmoji(stat.Key)} {stat.Key}: {stat.Value} trabajos");
        }

        Console.WriteLine($"\nTotal trabajos: {allJobs.Count}");

        // Mostrar trabajos fallidos
        var failedJobs = scheduler.GetJobs(JobState.Failed);
        if (failedJobs.Any())
        {
            Console.WriteLine("\n❌ TRABAJOS FALLIDOS:");
            foreach (var job in failedJobs)
            {
                Console.WriteLine($"  • {job.JobType}.{job.MethodName} - {job.Exception}");
            }
        }

        // Mostrar trabajos exitosos recientes
        var successfulJobs = scheduler.GetJobs(JobState.Succeeded).Take(5);
        if (successfulJobs.Any())
        {
            Console.WriteLine("\n✅ TRABAJOS EXITOSOS RECIENTES:");
            foreach (var job in successfulJobs)
            {
                Console.WriteLine($"  • {job.JobType}.{job.MethodName} - Completado en {job.ProcessedAt:HH:mm:ss}");
            }
        }

        Console.WriteLine("\n💡 BENEFICIOS DEL HANGFIRE PATTERN:");
        Console.WriteLine("  • Procesamiento asíncrono de tareas pesadas");
        Console.WriteLine("  • Programación de trabajos diferidos y recurrentes");
        Console.WriteLine("  • Reintentos automáticos con backoff exponencial");
        Console.WriteLine("  • Monitoreo y observabilidad de trabajos");
        Console.WriteLine("  • Escalabilidad con múltiples workers");
        Console.WriteLine("  • Persistencia de trabajos para recuperación");

        Console.WriteLine("\n⏳ Presiona cualquier tecla para detener el demo...");
        Console.ReadKey();
    }

    private static async Task MonitorJobsAsync(BackgroundJobScheduler scheduler)
    {
        var lastCount = 0;
        
        while (true)
        {
            await Task.Delay(5000);
            
            var jobs = scheduler.GetJobs();
            var processing = jobs.Count(j => j.State == JobState.Processing);
            var succeeded = jobs.Count(j => j.State == JobState.Succeeded);
            var failed = jobs.Count(j => j.State == JobState.Failed);
            var enqueued = jobs.Count(j => j.State == JobState.Enqueued);

            if (jobs.Count != lastCount)
            {
                Console.WriteLine($"📈 Estado actual - Procesando: {processing}, Exitosos: {succeeded}, Fallidos: {failed}, En cola: {enqueued}");
                lastCount = jobs.Count;
            }
        }
    }

    private static string GetStateEmoji(JobState state)
    {
        return state switch
        {
            JobState.Enqueued => "📋",
            JobState.Processing => "⚡",
            JobState.Succeeded => "✅",
            JobState.Failed => "❌",
            JobState.Scheduled => "⏰",
            JobState.Deleted => "🗑️",
            _ => "❓"
        };
    }
}
