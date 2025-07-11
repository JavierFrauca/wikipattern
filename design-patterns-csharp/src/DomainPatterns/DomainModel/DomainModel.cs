using System;

// Domain Model Pattern
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public void ApplyDiscount(decimal percent) => Price -= Price * percent;
}

// Ejemplo de uso
// var product = new Product { Name = "Libro", Price = 100 };
// product.ApplyDiscount(0.1m);
// Console.WriteLine(product.Price); // Output: 90
