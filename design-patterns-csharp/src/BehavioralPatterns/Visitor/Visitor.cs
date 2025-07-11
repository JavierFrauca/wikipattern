using System;

// Elemento
public interface IElement
{
    void Accept(IVisitor visitor);
}

// Elemento concreto
public class ConcreteElement : IElement
{
    public void Accept(IVisitor visitor) => visitor.Visit(this);
}

// Visitante
public interface IVisitor
{
    void Visit(ConcreteElement element);
}

// Visitante concreto
public class ConcreteVisitor : IVisitor
{
    public void Visit(ConcreteElement element) => Console.WriteLine("Visitando elemento");
}

// Ejemplo de uso
// var element = new ConcreteElement();
// var visitor = new ConcreteVisitor();
// element.Accept(visitor); // Output: Visitando elemento
