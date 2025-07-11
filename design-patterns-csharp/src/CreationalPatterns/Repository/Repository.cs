using System;

// Repositorio
public interface IRepository<T>
{
    void Add(T item);
    T Get(int id);
}

// Repositorio concreto
public class Repository<T> : IRepository<T>
{
    public void Add(T item) => Console.WriteLine($"Agregado: {item}");
    public T Get(int id) => default;
}

// Ejemplo de uso
// IRepository<string> repo = new Repository<string>();
// repo.Add("dato"); // Output: Agregado: dato
