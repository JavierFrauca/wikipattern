using System;

// Clase base
public abstract class AbstractClass
{
    public void TemplateMethod()
    {
        Step1();
        Step2();
    }
    protected abstract void Step1();
    protected abstract void Step2();
}

// Implementación concreta
public class ConcreteClass : AbstractClass
{
    protected override void Step1() => Console.WriteLine("Paso 1");
    protected override void Step2() => Console.WriteLine("Paso 2");
}

// Ejemplo de uso
// var obj = new ConcreteClass();
// obj.TemplateMethod(); // Output: Paso 1\nPaso 2
