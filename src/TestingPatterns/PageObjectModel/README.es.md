# Patrón Page Object Model

El patrón Page Object Model (POM) se usa en la automatización de pruebas de UI para encapsular elementos y acciones de la página en clases dedicadas.

## Estructura

- **Page Object**: Clase que representa una página o componente, con métodos para acciones y acceso a elementos.

## Ejemplo

```csharp
var page = new LoginPage();
page.EnterUsername("user");
page.EnterPassword("pass");
page.ClickLogin(); // Output: Usuario: user\nPassword: pass\nLogin pulsado
```

## Cuándo usarlo

- Para código de pruebas de UI mantenible y reutilizable.
- Para separar la lógica de prueba de la estructura de la UI.
