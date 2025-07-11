using System;

// Repository Pattern (Hexagonal)
public interface IRepository<T>
{
    void Add(T item);
    T Get(int id);
}

public class InMemoryRepository<T> : IRepository<T>
{
    public void Add(T item) => Console.WriteLine($"Added: {item}");
    public T Get(int id) => default;
}

// Example usage
// IRepository<string> repo = new InMemoryRepository<string>();
// repo.Add("data"); // Output: Added: data
