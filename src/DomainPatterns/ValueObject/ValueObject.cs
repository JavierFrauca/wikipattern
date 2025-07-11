using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

// ============================================================================
// VALUE OBJECT PATTERN - IMPLEMENTACIONES REALISTAS
// Ejemplos: Money, Email, Address, DateRange para sistemas empresariales
// ============================================================================

// ============================================================================
// 1. CLASE BASE PARA VALUE OBJECTS
// ============================================================================
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetAtomicValues();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !(left == right);
    }
}

// ============================================================================
// 2. MONEY VALUE OBJECT - PARA TRANSACCIONES FINANCIERAS
// ============================================================================
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private static readonly HashSet<string> SupportedCurrencies = new HashSet<string>
    {
        "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", "MXN"
    };

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));

        if (!SupportedCurrencies.Contains(currency.ToUpper()))
            throw new ArgumentException($"Currency '{currency}' is not supported", nameof(currency));

        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpper();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    public Money ApplyDiscount(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100", nameof(percentage));

        var discountAmount = Amount * (percentage / 100);
        return new Money(Amount - discountAmount, Currency);
    }

    public Money ApplyTax(decimal taxRate)
    {
        if (taxRate < 0)
            throw new ArgumentException("Tax rate cannot be negative", nameof(taxRate));

        var taxAmount = Amount * taxRate;
        return new Money(Amount + taxAmount, Currency);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }

    // Operadores para facilitar el uso
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);

    // Métodos de comparación
    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare different currencies");
        return Amount > other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare different currencies");
        return Amount < other.Amount;
    }
}

// ============================================================================
// 3. EMAIL VALUE OBJECT - PARA VALIDACIÓN DE EMAILS
// ============================================================================
public class Email : ValueObject
{
    public string Value { get; }
    public string LocalPart => Value.Split('@')[0];
    public string Domain => Value.Split('@')[1];

    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));

        if (email.Length > 254)
            throw new ArgumentException("Email address too long", nameof(email));

        Value = email;
    }

    public bool IsFromDomain(string domain)
    {
        return Domain.Equals(domain, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsPersonalEmail()
    {
        var personalDomains = new[] { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "icloud.com" };
        return personalDomains.Any(d => IsFromDomain(d));
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email?.Value;
    public static implicit operator Email(string email) => email != null ? new Email(email) : null;
}

// ============================================================================
// 4. ADDRESS VALUE OBJECT - PARA DIRECCIONES COMPLETAS
// ============================================================================
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be null or empty", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        State = state?.Trim();
        PostalCode = postalCode?.Trim();
        Country = country.Trim();
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Street, City };
        
        if (!string.IsNullOrEmpty(State))
            parts.Add(State);
        
        if (!string.IsNullOrEmpty(PostalCode))
            parts.Add(PostalCode);
        
        parts.Add(Country);

        return string.Join(", ", parts);
    }

    public bool IsInCountry(string country)
    {
        return Country.Equals(country, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsInState(string state)
    {
        return State?.Equals(state, StringComparison.OrdinalIgnoreCase) == true;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => GetFullAddress();
}

// ============================================================================
// 5. DATE RANGE VALUE OBJECT - PARA RANGOS DE FECHAS
// ============================================================================
public class DateRange : ValueObject
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public TimeSpan Duration => EndDate - StartDate;
    public int DaysCount => (int)Duration.TotalDays + 1;

    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be greater than end date");

        StartDate = startDate.Date; // Solo la fecha, sin hora
        EndDate = endDate.Date;
    }

    public bool Contains(DateTime date)
    {
        var dateOnly = date.Date;
        return dateOnly >= StartDate && dateOnly <= EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public DateRange? GetOverlap(DateRange other)
    {
        if (!Overlaps(other))
            return null;

        var overlapStart = StartDate > other.StartDate ? StartDate : other.StartDate;
        var overlapEnd = EndDate < other.EndDate ? EndDate : other.EndDate;

        return new DateRange(overlapStart, overlapEnd);
    }

    public bool IsWeekend(DateTime date)
    {
        if (!Contains(date))
            return false;

        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    public int GetBusinessDaysCount()
    {
        int businessDays = 0;
        for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
        {
            if (!IsWeekend(date))
                businessDays++;
        }
        return businessDays;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString()
    {
        return $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd} ({DaysCount} days)";
    }
}

// ============================================================================
// 6. PHONE NUMBER VALUE OBJECT
// ============================================================================
public class PhoneNumber : ValueObject
{
    public string CountryCode { get; }
    public string Number { get; }
    public string FullNumber => $"+{CountryCode}{Number}";

    private static readonly Regex PhoneRegex = new Regex(@"^\+?(\d{1,3})(\d{7,14})$", RegexOptions.Compiled);

    public PhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));

        // Limpiar el número
        var cleanNumber = Regex.Replace(phoneNumber, @"[^\d+]", "");

        var match = PhoneRegex.Match(cleanNumber);
        if (!match.Success)
            throw new ArgumentException($"Invalid phone number format: {phoneNumber}", nameof(phoneNumber));

        CountryCode = match.Groups[1].Value;
        Number = match.Groups[2].Value;
    }

    public string GetFormattedNumber()
    {
        // Formato simple para demostración
        if (CountryCode == "1" && Number.Length == 10)
        {
            // Formato US: +1 (555) 123-4567
            return $"+{CountryCode} ({Number.Substring(0, 3)}) {Number.Substring(3, 3)}-{Number.Substring(6)}";
        }

        return FullNumber;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CountryCode;
        yield return Number;
    }

    public override string ToString() => GetFormattedNumber();
}

// ============================================================================
// DEMO REALISTA DE VALUE OBJECTS
// ============================================================================
public static class ValueObjectDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("💎 DEMO: VALUE OBJECTS EN ACCIÓN");
        Console.WriteLine("================================");

        // Demo 1: Money Value Object
        Console.WriteLine("\n💰 MONEY VALUE OBJECT");
        try
        {
            var price = new Money(1299.99m, "USD");
            var discount = price.ApplyDiscount(15); // 15% descuento
            var finalPrice = discount.ApplyTax(0.08m); // 8% impuesto

            Console.WriteLine($"   Precio original: {price}");
            Console.WriteLine($"   Con descuento 15%: {discount}");
            Console.WriteLine($"   Precio final (con impuesto): {finalPrice}");

            // Operaciones con dinero
            var shipping = new Money(29.99m, "USD");
            var total = finalPrice + shipping;
            Console.WriteLine($"   Total con envío: {total}");

            // Comparaciones
            Console.WriteLine($"   ¿Precio mayor a $1000? {price.IsGreaterThan(new Money(1000, "USD"))}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Error: {ex.Message}");
        }

        // Demo 2: Email Value Object
        Console.WriteLine("\n📧 EMAIL VALUE OBJECT");
        try
        {
            var businessEmail = new Email("juan.perez@empresa.com");
            var personalEmail = new Email("juan@gmail.com");

            Console.WriteLine($"   Email empresarial: {businessEmail}");
            Console.WriteLine($"   Dominio: {businessEmail.Domain}");
            Console.WriteLine($"   ¿Es email personal? {businessEmail.IsPersonalEmail()}");

            Console.WriteLine($"   Email personal: {personalEmail}");
            Console.WriteLine($"   ¿Es email personal? {personalEmail.IsPersonalEmail()}");

            // Igualdad de value objects
            var sameEmail = new Email("juan.perez@empresa.com");
            Console.WriteLine($"   ¿Emails iguales? {businessEmail == sameEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Error: {ex.Message}");
        }

        // Demo 3: Address Value Object
        Console.WriteLine("\n🏠 ADDRESS VALUE OBJECT");
        try
        {
            var homeAddress = new Address(
                "Calle Principal 123",
                "Ciudad de México",
                "CDMX",
                "01234",
                "México"
            );

            var workAddress = new Address(
                "Av. Reforma 456",
                "Ciudad de México", 
                "CDMX",
                "01235",
                "México"
            );

            Console.WriteLine($"   Dirección casa: {homeAddress.GetFullAddress()}");
            Console.WriteLine($"   Dirección trabajo: {workAddress}");
            Console.WriteLine($"   ¿Misma ciudad? {homeAddress.City == workAddress.City}");
            Console.WriteLine($"   ¿Direcciones iguales? {homeAddress == workAddress}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Error: {ex.Message}");
        }

        // Demo 4: DateRange Value Object
        Console.WriteLine("\n📅 DATE RANGE VALUE OBJECT");
        try
        {
            var vacation = new DateRange(
                new DateTime(2024, 7, 15),
                new DateTime(2024, 7, 29)
            );

            var conference = new DateRange(
                new DateTime(2024, 7, 20),
                new DateTime(2024, 7, 22)
            );

            Console.WriteLine($"   Vacaciones: {vacation}");
            Console.WriteLine($"   Conferencia: {conference}");
            Console.WriteLine($"   Días de vacaciones: {vacation.DaysCount}");
            Console.WriteLine($"   Días laborables en vacaciones: {vacation.GetBusinessDaysCount()}");
            Console.WriteLine($"   ¿Se superponen? {vacation.Overlaps(conference)}");

            var overlap = vacation.GetOverlap(conference);
            if (overlap != null)
            {
                Console.WriteLine($"   Superposición: {overlap}");
            }

            Console.WriteLine($"   ¿25 julio está en vacaciones? {vacation.Contains(new DateTime(2024, 7, 25))}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Error: {ex.Message}");
        }

        // Demo 5: PhoneNumber Value Object
        Console.WriteLine("\n📱 PHONE NUMBER VALUE OBJECT");
        try
        {
            var phoneUS = new PhoneNumber("+1-555-123-4567");
            var phoneMX = new PhoneNumber("+52 55 1234 5678");

            Console.WriteLine($"   Teléfono US: {phoneUS.GetFormattedNumber()}");
            Console.WriteLine($"   Código país: {phoneUS.CountryCode}");
            Console.WriteLine($"   Número: {phoneUS.Number}");

            Console.WriteLine($"   Teléfono MX: {phoneMX}");

            // Igualdad
            var samePhone = new PhoneNumber("15551234567");
            Console.WriteLine($"   ¿Teléfonos iguales? {phoneUS == samePhone}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Error: {ex.Message}");
        }

        // Demo 6: Value Objects en colecciones
        Console.WriteLine("\n📋 VALUE OBJECTS EN COLECCIONES");
        var uniqueEmails = new HashSet<Email>
        {
            new Email("user1@ejemplo.com"),
            new Email("user2@ejemplo.com"),
            new Email("user1@ejemplo.com"), // Duplicado
        };
        Console.WriteLine($"   Emails únicos en HashSet: {uniqueEmails.Count}");

        var moneyAmounts = new List<Money>
        {
            new Money(100, "USD"),
            new Money(200, "USD"),
            new Money(100, "USD") // Duplicado
        };
        var uniqueAmounts = moneyAmounts.Distinct().ToList();
        Console.WriteLine($"   Cantidades únicas: {uniqueAmounts.Count}");

        Console.WriteLine("\n✅ Demo completado");
    }
}

// Para ejecutar el demo:
// ValueObjectDemo.RunDemo();
