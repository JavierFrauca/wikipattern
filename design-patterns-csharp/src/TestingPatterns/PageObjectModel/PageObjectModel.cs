using System;

// Page Object Model Pattern
public class LoginPage
{
    public void EnterUsername(string username) => Console.WriteLine($"Usuario: {username}");
    public void EnterPassword(string password) => Console.WriteLine($"Password: {password}");
    public void ClickLogin() => Console.WriteLine("Login pulsado");
}

// Ejemplo de uso
// var page = new LoginPage();
// page.EnterUsername("user");
// page.EnterPassword("pass");
// page.ClickLogin(); // Output: Usuario: user\nPassword: pass\nLogin pulsado
