using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

// ============================================================================
// SHARED KERNEL PATTERN - IMPLEMENTACIÓN REALISTA
// Ejemplo: E-commerce con múltiples bounded contexts compartiendo elementos comunes
// ============================================================================

// ============================================================================
// DOMAIN PRIMITIVES - TIPOS BÁSICOS COMPARTIDOS
// ============================================================================

/// <summary>
/// Value Object base para todos los bounded contexts
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject left, ValueObject right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !Equals(left, right);
    }
}

/// <summary>
/// Entity base para todos los bounded contexts
/// </summary>
public abstract class Entity<TId> where TId : IEquatable<TId>
{
    public TId Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public string UpdatedBy { get; protected set; }

    protected Entity(TId id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
    }

    protected Entity() { } // Para ORMs

    public override bool Equals(object obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    protected void MarkAsUpdated(string updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

/// <summary>
/// Aggregate Root base compartido
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : IEquatable<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// ============================================================================
// COMMON VALUE OBJECTS - COMPARTIDOS ENTRE CONTEXTOS
// ============================================================================

/// <summary>
/// Identificador único compartido entre todos los contextos
/// </summary>
public class EntityId : ValueObject
{
    public Guid Value { get; }

    public EntityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EntityId cannot be empty", nameof(value));
        
        Value = value;
    }

    public static EntityId New() => new(Guid.NewGuid());

    public static implicit operator Guid(EntityId entityId) => entityId.Value;
    public static implicit operator EntityId(Guid guid) => new(guid);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}

/// <summary>
/// Dinero - Value Object crítico compartido entre Order, Payment, Inventory
/// </summary>
public class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        Amount = Math.Round(amount, currency.DecimalPlaces);
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {Currency.Code} and {other.Currency.Code}");
        
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency.Code} and {other.Currency.Code}");
        
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare different currencies: {Currency.Code} and {other.Currency.Code}");
        
        return Amount > other.Amount;
    }

    public bool IsZero => Amount == 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount.ToString($"F{Currency.DecimalPlaces}")} {Currency.Code}";

    public static Money Zero(Currency currency) => new(0, currency);
}

/// <summary>
/// Moneda compartida
/// </summary>
public class Currency : ValueObject
{
    public string Code { get; }
    public string Name { get; }
    public int DecimalPlaces { get; }

    public Currency(string code, string name, int decimalPlaces = 2)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters", nameof(code));
        
        Code = code.ToUpper();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DecimalPlaces = decimalPlaces;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

    public override string ToString() => Code;

    // Monedas comunes
    public static Currency USD => new("USD", "US Dollar", 2);
    public static Currency EUR => new("EUR", "Euro", 2);
    public static Currency MXN => new("MXN", "Mexican Peso", 2);
}

/// <summary>
/// Email compartido entre User, Customer, Notification contexts
/// </summary>
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
        
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));
        
        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);
}

/// <summary>
/// Address compartido entre Customer, Shipping, Billing contexts
/// </summary>
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street?.Trim() ?? throw new ArgumentNullException(nameof(street));
        City = city?.Trim() ?? throw new ArgumentNullException(nameof(city));
        State = state?.Trim() ?? throw new ArgumentNullException(nameof(state));
        PostalCode = postalCode?.Trim() ?? throw new ArgumentNullException(nameof(postalCode));
        Country = country?.Trim() ?? throw new ArgumentNullException(nameof(country));

        if (string.IsNullOrWhiteSpace(Street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        if (string.IsNullOrWhiteSpace(City))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(PostalCode))
            throw new ArgumentException("PostalCode cannot be empty", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(Country))
            throw new ArgumentException("Country cannot be empty", nameof(country));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}

// ============================================================================
// COMMON DOMAIN EVENTS - EVENTOS COMPARTIDOS
// ============================================================================

/// <summary>
/// Interfaz base para eventos de dominio
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
    string EventType { get; }
}

/// <summary>
/// Evento base compartido
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

// ============================================================================
// COMMON EXCEPTIONS - EXCEPCIONES COMPARTIDAS
// ============================================================================

/// <summary>
/// Excepción base del dominio
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Excepción para reglas de negocio violadas
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message) 
        : base($"Business rule '{ruleName}' violated: {message}")
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Excepción para entidades no encontradas
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public string EntityId { get; }

    public EntityNotFoundException(string entityType, string entityId) 
        : base($"{entityType} with ID '{entityId}' was not found")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

// ============================================================================
// COMMON INTERFACES - INTERFACES COMPARTIDAS
// ============================================================================

/// <summary>
/// Repositorio base compartido
/// </summary>
public interface IRepository<TEntity, TId> 
    where TEntity : AggregateRoot<TId> 
    where TId : IEquatable<TId>
{
    Task<TEntity> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work compartido
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Event Bus compartido
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent;
    Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
}

// ============================================================================
// COMMON SPECIFICATIONS - ESPECIFICACIONES COMPARTIDAS
// ============================================================================

/// <summary>
/// Specification pattern base
/// </summary>
public abstract class Specification<T>
{
    public abstract bool IsSatisfiedBy(T entity);

    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    public Specification<T> Or(Specification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

public class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(T entity)
    {
        return _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
    }
}

public class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override bool IsSatisfiedBy(T entity)
    {
        return _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
    }
}

public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override bool IsSatisfiedBy(T entity)
    {
        return !_specification.IsSatisfiedBy(entity);
    }
}

// ============================================================================
// COMMON UTILITIES - UTILIDADES COMPARTIDAS
// ============================================================================

/// <summary>
/// Utilidades para fechas compartidas
/// </summary>
public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return dateTime.Kind == DateTimeKind.Utc ? dateTime : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
    }

    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);
    }

    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999, dateTime.Kind);
    }
}

/// <summary>
/// Utilidades para validación compartidas
/// </summary>
public static class ValidationExtensions
{
    public static void ThrowIfNull<T>(this T obj, string paramName) where T : class
    {
        if (obj == null)
            throw new ArgumentNullException(paramName);
    }

    public static void ThrowIfNullOrEmpty(this string str, string paramName)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new ArgumentException("Value cannot be null or empty", paramName);
    }

    public static void ThrowIfNegativeOrZero(this decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException("Value must be positive", paramName);
    }
}

// ============================================================================
// BOUNDED CONTEXTS USANDO SHARED KERNEL
// ============================================================================

namespace OrderContext
{
    public class Order : AggregateRoot<EntityId>
    {
        public EntityId CustomerId { get; private set; }
        public Money TotalAmount { get; private set; }
        public Address ShippingAddress { get; private set; }
        public OrderStatus Status { get; private set; }
        private readonly List<OrderItem> _items = new();

        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        public Order(EntityId customerId, Address shippingAddress) : base(EntityId.New())
        {
            CustomerId = customerId.ThrowIfNull(nameof(customerId));
            ShippingAddress = shippingAddress.ThrowIfNull(nameof(shippingAddress));
            Status = OrderStatus.Pending;
            TotalAmount = Money.Zero(Currency.USD);
        }

        public void AddItem(EntityId productId, string productName, Money unitPrice, int quantity)
        {
            if (Status != OrderStatus.Pending)
                throw new BusinessRuleViolationException("CannotModifyNonPendingOrder", 
                    "Cannot modify order that is not in pending status");

            var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.ChangeQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                _items.Add(new OrderItem(productId, productName, unitPrice, quantity));
            }

            RecalculateTotal();
            AddDomainEvent(new OrderItemAddedEvent(Id, productId, quantity));
        }

        private void RecalculateTotal()
        {
            TotalAmount = _items.Aggregate(Money.Zero(Currency.USD), 
                (total, item) => total.Add(item.TotalPrice));
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
                throw new BusinessRuleViolationException("CannotConfirmNonPendingOrder", 
                    "Cannot confirm order that is not pending");

            if (!_items.Any())
                throw new BusinessRuleViolationException("CannotConfirmEmptyOrder", 
                    "Cannot confirm order with no items");

            Status = OrderStatus.Confirmed;
            AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, TotalAmount));
        }
    }

    public class OrderItem : Entity<EntityId>
    {
        public EntityId ProductId { get; private set; }
        public string ProductName { get; private set; }
        public Money UnitPrice { get; private set; }
        public int Quantity { get; private set; }
        public Money TotalPrice => UnitPrice.Multiply(Quantity);

        public OrderItem(EntityId productId, string productName, Money unitPrice, int quantity) 
            : base(EntityId.New())
        {
            ProductId = productId.ThrowIfNull(nameof(productId));
            ProductName = productName.ThrowIfNullOrEmpty(nameof(productName));
            UnitPrice = unitPrice.ThrowIfNull(nameof(unitPrice));
            Quantity = quantity;

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
        }

        public void ChangeQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

            Quantity = newQuantity;
            MarkAsUpdated();
        }
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }

    public class OrderConfirmedEvent : DomainEvent
    {
        public override string EventType => "OrderConfirmed";
        public EntityId OrderId { get; }
        public EntityId CustomerId { get; }
        public Money TotalAmount { get; }

        public OrderConfirmedEvent(EntityId orderId, EntityId customerId, Money totalAmount)
        {
            OrderId = orderId;
            CustomerId = customerId;
            TotalAmount = totalAmount;
        }
    }

    public class OrderItemAddedEvent : DomainEvent
    {
        public override string EventType => "OrderItemAdded";
        public EntityId OrderId { get; }
        public EntityId ProductId { get; }
        public int Quantity { get; }

        public OrderItemAddedEvent(EntityId orderId, EntityId productId, int quantity)
        {
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
        }
    }
}

namespace CustomerContext
{
    public class Customer : AggregateRoot<EntityId>
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Email Email { get; private set; }
        public Address BillingAddress { get; private set; }
        public CustomerStatus Status { get; private set; }

        public string FullName => $"{FirstName} {LastName}";

        public Customer(string firstName, string lastName, Email email, Address billingAddress) 
            : base(EntityId.New())
        {
            FirstName = firstName.ThrowIfNullOrEmpty(nameof(firstName));
            LastName = lastName.ThrowIfNullOrEmpty(nameof(lastName));
            Email = email.ThrowIfNull(nameof(email));
            BillingAddress = billingAddress.ThrowIfNull(nameof(billingAddress));
            Status = CustomerStatus.Active;

            AddDomainEvent(new CustomerCreatedEvent(Id, Email, FullName));
        }

        public void UpdateEmail(Email newEmail)
        {
            var oldEmail = Email;
            Email = newEmail.ThrowIfNull(nameof(newEmail));
            
            AddDomainEvent(new CustomerEmailChangedEvent(Id, oldEmail, newEmail));
            MarkAsUpdated();
        }

        public void Deactivate()
        {
            if (Status == CustomerStatus.Inactive)
                throw new BusinessRuleViolationException("CustomerAlreadyInactive", 
                    "Customer is already inactive");

            Status = CustomerStatus.Inactive;
            AddDomainEvent(new CustomerDeactivatedEvent(Id));
        }
    }

    public enum CustomerStatus
    {
        Active,
        Inactive,
        Suspended
    }

    public class CustomerCreatedEvent : DomainEvent
    {
        public override string EventType => "CustomerCreated";
        public EntityId CustomerId { get; }
        public Email Email { get; }
        public string FullName { get; }

        public CustomerCreatedEvent(EntityId customerId, Email email, string fullName)
        {
            CustomerId = customerId;
            Email = email;
            FullName = fullName;
        }
    }

    public class CustomerEmailChangedEvent : DomainEvent
    {
        public override string EventType => "CustomerEmailChanged";
        public EntityId CustomerId { get; }
        public Email OldEmail { get; }
        public Email NewEmail { get; }

        public CustomerEmailChangedEvent(EntityId customerId, Email oldEmail, Email newEmail)
        {
            CustomerId = customerId;
            OldEmail = oldEmail;
            NewEmail = newEmail;
        }
    }

    public class CustomerDeactivatedEvent : DomainEvent
    {
        public override string EventType => "CustomerDeactivated";
        public EntityId CustomerId { get; }

        public CustomerDeactivatedEvent(EntityId customerId)
        {
            CustomerId = customerId;
        }
    }
}

namespace PaymentContext
{
    public class Payment : AggregateRoot<EntityId>
    {
        public EntityId OrderId { get; private set; }
        public Money Amount { get; private set; }
        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string TransactionId { get; private set; }

        public Payment(EntityId orderId, Money amount, PaymentMethod method) : base(EntityId.New())
        {
            OrderId = orderId.ThrowIfNull(nameof(orderId));
            Amount = amount.ThrowIfNull(nameof(amount));
            Method = method.ThrowIfNull(nameof(method));
            Status = PaymentStatus.Pending;

            if (amount.IsZero)
                throw new BusinessRuleViolationException("PaymentAmountMustBePositive", 
                    "Payment amount must be greater than zero");
        }

        public void Process(string transactionId)
        {
            if (Status != PaymentStatus.Pending)
                throw new BusinessRuleViolationException("PaymentAlreadyProcessed", 
                    "Payment has already been processed");

            TransactionId = transactionId.ThrowIfNullOrEmpty(nameof(transactionId));
            Status = PaymentStatus.Completed;

            AddDomainEvent(new PaymentCompletedEvent(Id, OrderId, Amount, TransactionId));
            MarkAsUpdated();
        }

        public void Fail(string reason)
        {
            if (Status != PaymentStatus.Pending)
                throw new BusinessRuleViolationException("PaymentAlreadyProcessed", 
                    "Payment has already been processed");

            Status = PaymentStatus.Failed;
            AddDomainEvent(new PaymentFailedEvent(Id, OrderId, Amount, reason));
            MarkAsUpdated();
        }
    }

    public class PaymentMethod : ValueObject
    {
        public string Type { get; }
        public string Provider { get; }
        public string Last4Digits { get; }

        public PaymentMethod(string type, string provider, string last4Digits = null)
        {
            Type = type.ThrowIfNullOrEmpty(nameof(type));
            Provider = provider.ThrowIfNullOrEmpty(nameof(provider));
            Last4Digits = last4Digits;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Type;
            yield return Provider;
            yield return Last4Digits;
        }

        public static PaymentMethod CreditCard(string provider, string last4Digits) 
            => new("CreditCard", provider, last4Digits);
        
        public static PaymentMethod PayPal() => new("PayPal", "PayPal");
        public static PaymentMethod BankTransfer() => new("BankTransfer", "Bank");
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public class PaymentCompletedEvent : DomainEvent
    {
        public override string EventType => "PaymentCompleted";
        public EntityId PaymentId { get; }
        public EntityId OrderId { get; }
        public Money Amount { get; }
        public string TransactionId { get; }

        public PaymentCompletedEvent(EntityId paymentId, EntityId orderId, Money amount, string transactionId)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
            TransactionId = transactionId;
        }
    }

    public class PaymentFailedEvent : DomainEvent
    {
        public override string EventType => "PaymentFailed";
        public EntityId PaymentId { get; }
        public EntityId OrderId { get; }
        public Money Amount { get; }
        public string Reason { get; }

        public PaymentFailedEvent(EntityId paymentId, EntityId orderId, Money amount, string reason)
        {
            PaymentId = paymentId;
            OrderId = orderId;
            Amount = amount;
            Reason = reason;
        }
    }
}

// ============================================================================
// DEMO REALISTA DEL SHARED KERNEL
// ============================================================================

public static class SharedKernelDemo
{
    public static async Task RunDemo()
    {
        Console.WriteLine("🎯 DEMO: SHARED KERNEL PATTERN");
        Console.WriteLine("===============================");
        Console.WriteLine("Demostración de elementos compartidos entre bounded contexts\n");

        try
        {
            // 1. Crear elementos del shared kernel
            Console.WriteLine("🔧 CREANDO ELEMENTOS DEL SHARED KERNEL:");
            Console.WriteLine("==========================================");

            var customerId = EntityId.New();
            var orderId = EntityId.New();
            var productId = EntityId.New();

            var customerEmail = new Email("juan.perez@example.com");
            var shippingAddress = new Address(
                "Av. Reforma 123",
                "Ciudad de México",
                "CDMX",
                "01000",
                "México"
            );

            var price = new Money(999.99m, Currency.USD);
            var paymentMethod = PaymentMethod.CreditCard("Visa", "4444");

            Console.WriteLine($"✅ EntityIds creados: Customer={customerId}, Order={orderId}");
            Console.WriteLine($"✅ Email: {customerEmail}");
            Console.WriteLine($"✅ Address: {shippingAddress}");
            Console.WriteLine($"✅ Money: {price}");
            Console.WriteLine($"✅ PaymentMethod: {paymentMethod.Type} - {paymentMethod.Provider}");

            // 2. Usar shared kernel en Customer Context
            Console.WriteLine("\n👤 CUSTOMER CONTEXT:");
            Console.WriteLine("=====================");

            var customer = new CustomerContext.Customer(
                "Juan",
                "Pérez",
                customerEmail,
                shippingAddress
            );

            Console.WriteLine($"✅ Customer creado: {customer.FullName}");
            Console.WriteLine($"   ID: {customer.Id}");
            Console.WriteLine($"   Email: {customer.Email}");
            Console.WriteLine($"   Status: {customer.Status}");
            Console.WriteLine($"   Eventos generados: {customer.DomainEvents.Count}");

            // 3. Usar shared kernel en Order Context
            Console.WriteLine("\n🛒 ORDER CONTEXT:");
            Console.WriteLine("==================");

            var order = new OrderContext.Order(customer.Id, shippingAddress);
            
            order.AddItem(productId, "Laptop Gaming", price, 1);
            order.AddItem(EntityId.New(), "Mouse Gaming", new Money(49.99m, Currency.USD), 2);

            Console.WriteLine($"✅ Order creada: {order.Id}");
            Console.WriteLine($"   Customer: {order.CustomerId}");
            Console.WriteLine($"   Items: {order.Items.Count}");
            Console.WriteLine($"   Total: {order.TotalAmount}");
            Console.WriteLine($"   Status: {order.Status}");
            Console.WriteLine($"   Eventos generados: {order.DomainEvents.Count}");

            order.Confirm();
            Console.WriteLine($"✅ Order confirmada, nuevos eventos: {order.DomainEvents.Count}");

            // 4. Usar shared kernel en Payment Context
            Console.WriteLine("\n💳 PAYMENT CONTEXT:");
            Console.WriteLine("====================");

            var payment = new PaymentContext.Payment(order.Id, order.TotalAmount, paymentMethod);
            
            Console.WriteLine($"✅ Payment creado: {payment.Id}");
            Console.WriteLine($"   Order: {payment.OrderId}");
            Console.WriteLine($"   Amount: {payment.Amount}");
            Console.WriteLine($"   Method: {payment.Method.Type}");
            Console.WriteLine($"   Status: {payment.Status}");

            payment.Process($"TXN-{Guid.NewGuid().ToString()[..8]}");
            Console.WriteLine($"✅ Payment procesado: {payment.TransactionId}");
            Console.WriteLine($"   Nuevos eventos: {payment.DomainEvents.Count}");

            // 5. Demostrar operaciones con Money
            Console.WriteLine("\n💰 OPERACIONES CON MONEY:");
            Console.WriteLine("==========================");

            var price1 = new Money(100.50m, Currency.USD);
            var price2 = new Money(49.99m, Currency.USD);
            var total = price1.Add(price2);
            var discounted = total.Multiply(0.9m);

            Console.WriteLine($"Precio 1: {price1}");
            Console.WriteLine($"Precio 2: {price2}");
            Console.WriteLine($"Total: {total}");
            Console.WriteLine($"Con descuento 10%: {discounted}");
            Console.WriteLine($"¿Es mayor que 100? {discounted.IsGreaterThan(new Money(100m, Currency.USD))}");

            // 6. Demostrar Value Objects
            Console.WriteLine("\n🔍 VALUE OBJECTS COMPARISON:");
            Console.WriteLine("=============================");

            var email1 = new Email("test@example.com");
            var email2 = new Email("TEST@EXAMPLE.COM");
            var email3 = new Email("other@example.com");

            Console.WriteLine($"Email1: {email1}");
            Console.WriteLine($"Email2: {email2}");
            Console.WriteLine($"Email3: {email3}");
            Console.WriteLine($"Email1 == Email2: {email1 == email2}");
            Console.WriteLine($"Email1 == Email3: {email1 == email3}");

            var addr1 = new Address("Main St 123", "City", "State", "12345", "Country");
            var addr2 = new Address("Main St 123", "City", "State", "12345", "Country");
            var addr3 = new Address("Other St 456", "City", "State", "12345", "Country");

            Console.WriteLine($"\nAddress1 == Address2: {addr1 == addr2}");
            Console.WriteLine($"Address1 == Address3: {addr1 == addr3}");

            // 7. Mostrar todos los eventos generados
            Console.WriteLine("\n📢 EVENTOS DE DOMINIO GENERADOS:");
            Console.WriteLine("=================================");

            var allEvents = new List<IDomainEvent>();
            allEvents.AddRange(customer.DomainEvents);
            allEvents.AddRange(order.DomainEvents);
            allEvents.AddRange(payment.DomainEvents);

            foreach (var domainEvent in allEvents.OrderBy(e => e.OccurredAt))
            {
                Console.WriteLine($"🔔 {domainEvent.EventType} - {domainEvent.OccurredAt:HH:mm:ss.fff}");
                Console.WriteLine($"   ID: {domainEvent.Id}");
            }

            Console.WriteLine($"\n📊 RESUMEN: {allEvents.Count} eventos generados en total");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en demo: {ex.Message}");
            if (ex is DomainException domainEx)
            {
                Console.WriteLine($"   Tipo: {domainEx.GetType().Name}");
            }
        }

        Console.WriteLine("\n✅ Demo completado");
        Console.WriteLine("\n💡 BENEFICIOS DEL SHARED KERNEL:");
        Console.WriteLine("  • Consistencia entre bounded contexts");
        Console.WriteLine("  • Reutilización de lógica común");
        Console.WriteLine("  • Menos duplicación de código");
        Console.WriteLine("  • Evolución coordinada de elementos compartidos");
        Console.WriteLine("  • Mantenimiento centralizado de reglas comunes");
    }
}
