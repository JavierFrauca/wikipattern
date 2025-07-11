using System;

// Aggregate Pattern
public class Order
{
    public int Id { get; set; }
}

public class OrderAggregate
{
    public Order Order { get; set; }
    public void AddOrder(Order order) => Order = order;
}

// Ejemplo de uso
// var agg = new OrderAggregate();
// agg.AddOrder(new Order { Id = 1 });
// Console.WriteLine(agg.Order.Id); // Output: 1
