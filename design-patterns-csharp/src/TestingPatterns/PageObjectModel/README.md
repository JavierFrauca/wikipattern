# Page Object Model Pattern

The Page Object Model (POM) pattern is used in UI test automation to encapsulate page elements and actions in dedicated classes.

## Structure

- **Page Object**: Class representing a page or component, with methods for actions and element access.

## Example

```csharp
var page = new LoginPage();
page.EnterUsername("user");
page.EnterPassword("pass");
page.ClickLogin(); // Output: Usuario: user\nPassword: pass\nLogin pulsado
```

## When to use

- For maintainable and reusable UI test code.
- To separate test logic from UI structure.
