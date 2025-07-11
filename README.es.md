# 🎓 Patrones de Diseño y Arquitectura en C# - Repositorio Didáctico

[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/download)
[![C#](https://img.shields.io/badge/C%23-12.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

> 🌐 **Language / Idioma**: [🇺🇸 English](README.en.md) | [🇪🇸 Español](README.md) | [🇪🇸 Español (Original)](README.es.md)

---

## 📖 Acerca de este Repositorio

Este repositorio es una **colección educativa completa** de patrones de diseño y arquitectura implementados en **C# 12** con **.NET 8+**. Ha sido creado específicamente con **fines didácticos** para ayudar a desarrolladores de todos los niveles a comprender, aprender y aplicar estos patrones fundamentales en sus proyectos profesionales.

### 🎯 Objetivos Educativos

- **📚 Aprendizaje Práctico**: Cada patrón incluye implementaciones realistas con ejemplos del mundo real
- **💡 Comprensión Profunda**: Explicaciones detalladas de cuándo, cómo y por qué usar cada patrón
- **🔧 Código Production-Ready**: Implementaciones robustas que pueden servir como referencia para proyectos reales
- **🎓 Progresión Estructurada**: Desde patrones básicos hasta arquitecturas complejas
- **💼 Aplicación Profesional**: Ejemplos contextualizados en escenarios empresariales reales

### 🌟 Características Destacadas

- ✅ **Implementaciones Completas**: Cada patrón incluye código funcional y demos interactivos
- ✅ **Ejemplos Realistas**: Sistemas de e-commerce, trading, notificaciones, pagos, etc.
- ✅ **Documentación Bilingüe**: Español e inglés para mayor accesibilidad
- ✅ **Thread-Safety**: Consideraciones de concurrencia donde es relevante
- ✅ **Mejores Prácticas**: Siguiendo convenciones de C# y .NET moderno
- ✅ **Casos de Uso Claros**: Cuándo usar y cuándo NO usar cada patrón
- ✅ **Progresión de Dificultad**: Desde conceptos básicos hasta arquitecturas avanzadas

---

## 📋 Índice Completo de Patrones

### 🏗️ Patrones Creacionales (Creational Patterns)
*Enfocados en la creación de objetos de manera flexible y reutilizable*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**Factory Method**](src/CreationalPatterns/FactoryMethod/) | Crea objetos sin especificar la clase exacta | 📧 Sistema de notificaciones empresariales | 🟢 Básico |
| [**Abstract Factory**](src/CreationalPatterns/AbstractFactory/) | Familias de objetos relacionados | 🖥️ Interfaces de usuario multiplataforma | 🟡 Intermedio |
| [**Builder**](src/CreationalPatterns/Builder/) | Construcción paso a paso de objetos complejos | 🖥️ Configurador de PCs personalizadas | 🟢 Básico |
| [**Singleton**](src/CreationalPatterns/Singleton/) | Una única instancia global | ⚙️ Configuración de aplicación y cache | 🟢 Básico |
| [**Prototype**](src/CreationalPatterns/Prototype/) | Clonación de objetos existentes | 📄 Plantillas de documentos | 🟡 Intermedio |

### 🔧 Patrones Estructurales (Structural Patterns)
*Enfocados en la composición de clases y objetos*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**Adapter**](src/StructuralPatterns/Adapter/) | Hace compatibles interfaces incompatibles | 💳 Integración de sistemas de pago | 🟢 Básico |
| [**Bridge**](src/StructuralPatterns/Bridge/) | Separa abstracción de implementación | 🎮 Reproductores multimedia multiplataforma | 🟡 Intermedio |
| [**Composite**](src/StructuralPatterns/Composite/) | Estructura de árbol para objetos | 📁 Sistema de archivos y directorios | 🟡 Intermedio |
| [**Decorator**](src/StructuralPatterns/Decorator/) | Añade funcionalidad dinámicamente | ☕ Personalizador de bebidas | 🟢 Básico |
| [**Facade**](src/StructuralPatterns/Facade/) | Interfaz simplificada para subsistemas | 🏠 Sistema de automatización del hogar | 🟢 Básico |
| [**Flyweight**](src/StructuralPatterns/Flyweight/) | Minimiza uso de memoria compartiendo datos | 🎮 Motor de renderizado de partículas | 🔴 Avanzado |
| [**Proxy**](src/StructuralPatterns/Proxy/) | Placeholder que controla acceso a otro objeto | 🖼️ Carga lazy de imágenes con cache | 🟡 Intermedio |

### 🎭 Patrones de Comportamiento (Behavioral Patterns)
*Enfocados en algoritmos y asignación de responsabilidades*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**Chain of Responsibility**](src/BehavioralPatterns/ChainOfResponsibility/) | Cadena de manejadores para procesar solicitudes | 🎫 Sistema de aprobación de solicitudes | 🟡 Intermedio |
| [**Command**](src/BehavioralPatterns/Command/) | Encapsula solicitudes como objetos | 🎮 Editor con undo/redo | 🟡 Intermedio |
| [**Interpreter**](src/BehavioralPatterns/Interpreter/) | Interpreta lenguajes o expresiones | 🧮 Calculadora de expresiones matemáticas | 🔴 Avanzado |
| [**Iterator**](src/BehavioralPatterns/Iterator/) | Acceso secuencial a elementos de una colección | 📊 Recorrido de estructuras de datos | 🟢 Básico |
| [**Mediator**](src/BehavioralPatterns/Mediator/) | Define comunicación entre objetos | 💬 Sistema de chat multiusuario | 🟡 Intermedio |
| [**Memento**](src/BehavioralPatterns/Memento/) | Captura y restaura estados de objetos | 🎮 Sistema de guardado de juegos | 🟡 Intermedio |
| [**Observer**](src/BehavioralPatterns/Observer/) | Notifica cambios a múltiples observadores | 📈 Sistema de trading en tiempo real | 🟢 Básico |
| [**State**](src/BehavioralPatterns/State/) | Altera comportamiento según estado interno | 🚦 Máquina de estados de pedidos | 🟡 Intermedio |
| [**Strategy**](src/BehavioralPatterns/Strategy/) | Familia de algoritmos intercambiables | 💰 Sistema de precios dinámico | 🟢 Básico |
| [**Template Method**](src/BehavioralPatterns/TemplateMethod/) | Define esqueleto de algoritmo en clase base | 🍳 Recetas de cocina personalizables | 🟢 Básico |
| [**Visitor**](src/BehavioralPatterns/Visitor/) | Operaciones sobre estructura de objetos | 📊 Analizador de código fuente | 🔴 Avanzado |

### 🏛️ Patrones Arquitectónicos (Architectural Patterns)
*Enfocados en la estructura general de aplicaciones*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**MVC**](src/ArchitecturalPatterns/MVC/) | Model-View-Controller | 🌐 Aplicación web de gestión | 🟡 Intermedio |
| [**MVVM**](src/ArchitecturalPatterns/MVVM/) | Model-View-ViewModel | 📱 Aplicación de escritorio con bindings | 🟡 Intermedio |
| [**CQRS**](src/ArchitecturalPatterns/CQRS/) | Command Query Responsibility Segregation | 🛒 Sistema de e-commerce | 🔴 Avanzado |
| [**Event Sourcing**](src/ArchitecturalPatterns/EventSourcing/) | Almacena eventos en lugar de estado | 🏦 Sistema bancario con auditoría | 🔴 Avanzado |
| [**Repository**](src/ArchitecturalPatterns/Repository/) | Abstrae acceso a datos | 🗄️ Gestor de datos empresariales | 🟡 Intermedio |
| [**Unit of Work**](src/ArchitecturalPatterns/UnitOfWork/) | Mantiene lista de objetos afectados por transacción | 💾 Transacciones de base de datos | 🟡 Intermedio |
| [**Dependency Injection**](src/ArchitecturalPatterns/DependencyInjection/) | Inversión de control de dependencias | 🔧 Framework de inyección personalizado | 🟡 Intermedio |
| [**Hexagonal**](src/ArchitecturalPatterns/Hexagonal/) | Arquitectura de puertos y adaptadores | 🏗️ Sistema modular empresarial | 🔴 Avanzado |

### 🎯 Patrones de Dominio (Domain Patterns - DDD)
*Enfocados en el modelado del dominio de negocio*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**Domain Model**](src/DomainPatterns/DomainModel/) | Modelo rico del dominio de negocio | 🏢 Sistema de gestión empresarial | 🟡 Intermedio |
| [**Entity**](src/DomainPatterns/Entity/) | Objetos con identidad única | 👤 Gestión de usuarios y perfiles | 🟢 Básico |
| [**Value Object**](src/DomainPatterns/ValueObject/) | Objetos inmutables sin identidad | 💰 Money, Email, Address | 🟢 Básico |
| [**Aggregate**](src/DomainPatterns/Aggregate/) | Conjunto de entidades tratadas como una unidad | 🛒 Carrito de compras completo | 🟡 Intermedio |
| [**Domain Service**](src/DomainPatterns/DomainService/) | Lógica de negocio que no pertenece a entidades | 🧮 Calculadora de impuestos | 🟡 Intermedio |
| [**Application Service**](src/DomainPatterns/ApplicationService/) | Orquesta casos de uso de aplicación | 🎯 Coordinador de procesos de negocio | 🟡 Intermedio |
| [**Domain Events**](src/DomainPatterns/DomainEvents/) | Eventos significativos del dominio | 📢 Sistema de eventos empresariales | 🔴 Avanzado |
| [**Specification**](src/DomainPatterns/Specification/) | Encapsula reglas de negocio | ✅ Validador de reglas complejas | 🟡 Intermedio |

### 🛡️ Patrones de Resiliencia (Resilience Patterns)
*Enfocados en la tolerancia a fallos y estabilidad*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**Circuit Breaker**](src/ResiliencePatterns/CircuitBreaker/) | Previene fallos en cascada | 💳 Protección de servicios de pago | 🟡 Intermedio |
| [**Retry**](src/ResiliencePatterns/Retry/) | Reintenta operaciones fallidas | 🔄 Cliente HTTP resiliente | 🟢 Básico |
| [**Bulkhead**](src/ResiliencePatterns/Bulkhead/) | Aísla recursos críticos | 🚢 Aislamiento de pools de conexiones | 🔴 Avanzado |
| [**Timeout**](src/ResiliencePatterns/Timeout/) | Previene operaciones colgadas | ⏰ Cliente con timeouts configurable | 🟢 Básico |
| [**Rate Limiting**](src/ResiliencePatterns/RateLimiting/) | Controla tasa de solicitudes | 🚦 API con límites de velocidad | 🟡 Intermedio |
| [**Fallback**](src/ResiliencePatterns/Fallback/) | Respuesta alternativa en caso de fallo | 🔄 Servicio con respaldo local | 🟡 Intermedio |

### ⚡ Patrones de Concurrencia (Concurrency Patterns)
*Enfocados en programación concurrente y paralela*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**Producer-Consumer**](src/ConcurrencyPatterns/ProducerConsumer/) | Productores y consumidores con cola | 📦 Sistema de procesamiento de pedidos | 🟡 Intermedio |
| [**Thread Pool**](src/ConcurrencyPatterns/ThreadPool/) | Pool de hilos reutilizables | 🔄 Procesador de tareas en paralelo | 🟡 Intermedio |
| [**Async-Await**](src/ConcurrencyPatterns/AsyncAwait/) | Programación asíncrona moderna | 🛒 Procesamiento de órdenes async | 🟡 Intermedio |
| [**Pipeline**](src/ConcurrencyPatterns/Pipeline/) | Procesamiento en etapas paralelas | 🏭 Pipeline de procesamiento de datos | 🔴 Avanzado |

### 🧪 Patrones de Testing
*Enfocados en pruebas y calidad de código*

| Patrón | Descripción | Ejemplo Didáctico | Nivel |
|--------|-------------|-------------------|-------|
| [**TDD**](src/TestingPatterns/TDD/) | Test-Driven Development | ✅ Desarrollo guiado por pruebas | 🟢 Básico |
| [**BDD**](src/TestingPatterns/BDD/) | Behavior-Driven Development | 📋 Especificaciones ejecutables | 🟡 Intermedio |
| [**AAA Pattern**](src/TestingPatterns/AAA/) | Arrange-Act-Assert | 🎯 Estructura de pruebas unitarias | 🟢 Básico |
| [**Test Doubles**](src/TestingPatterns/TestDoubles/) | Mocks, Stubs, Fakes | 🎭 Dobles de prueba para aislamiento | 🟡 Intermedio |
| [**Page Object Model**](src/TestingPatterns/PageObjectModel/) | Abstracción de páginas web para pruebas | 🌐 Automatización de pruebas UI | 🟡 Intermedio |

---

## 🚀 Comenzando

### 📋 Prerrequisitos

- **.NET 8.0 SDK** o superior
- **C# 12** compatible IDE (Visual Studio 2022, VS Code, Rider)
- **Git** para clonar el repositorio

### 📥 Instalación

```bash
# Clonar el repositorio
git clone https://github.com/tu-usuario/design-patterns-csharp.git

# Navegar al directorio
cd design-patterns-csharp

# Restaurar paquetes (si es necesario)
dotnet restore
```

### 🎯 Cómo Usar Este Repositorio

#### 📚 Para Estudiantes

1. **Comienza con los patrones básicos** (🟢) como Factory Method, Observer, Strategy
2. **Lee la documentación** de cada patrón antes de ver el código
3. **Ejecuta los demos** para ver los patrones en acción
4. **Experimenta modificando** los ejemplos para entender mejor
5. **Progresa gradualmente** hacia patrones más complejos

#### 👨‍🏫 Para Instructores

- Cada patrón incluye **material didáctico completo**
- **Ejemplos progresivos** desde conceptos básicos hasta implementaciones complejas
- **Casos de uso reales** que los estudiantes pueden relacionar
- **Ejercicios implícitos** a través de variaciones en los demos

#### 💼 Para Profesionales

- **Referencia rápida** de implementaciones production-ready
- **Mejores prácticas** aplicadas en cada ejemplo
- **Consideraciones de rendimiento** y thread-safety
- **Código reutilizable** para proyectos reales

### 🎮 Ejecutando los Ejemplos

Cada patrón incluye demos interactivos:

```csharp
// Ejemplo: Factory Method
NotificationDemo.RunDemo();

// Ejemplo: Observer Pattern  
await StockTradingDemo.RunDemo();

// Ejemplo: Strategy Pattern
StrategyPatternDemo.RunDemo();

// Ejemplo: Async/Await
await AsyncAwaitPatternDemo.RunDemo();
```

---

## 🏆 Metodología de Aprendizaje

### 📈 Progresión Sugerida

1. **🟢 Nivel Básico (1-2 semanas)**
   - Factory Method, Singleton, Strategy, Observer
   - Template Method, Iterator, Decorator

2. **🟡 Nivel Intermedio (2-3 semanas)**
   - Builder, Adapter, Command, State
   - Repository, Unit of Work, MVC/MVVM

3. **🔴 Nivel Avanzado (3-4 semanas)**
   - CQRS, Event Sourcing, Hexagonal Architecture
   - Circuit Breaker, Pipeline, Domain Events

### 🎯 Enfoque Didáctico

Cada patrón sigue una estructura educativa consistente:

1. **📖 Introducción conceptual** - ¿Qué problema resuelve?
2. **🎯 Cuándo usarlo** - Escenarios apropiados
3. **⚠️ Cuándo NO usarlo** - Anti-patrones y advertencias
4. **💡 Implementación paso a paso** - Construcción gradual
5. **🔧 Ejemplo realista** - Caso de uso del mundo real
6. **✅ Demo interactivo** - Código ejecutable
7. **📊 Análisis de resultados** - Lecciones aprendidas
8. **🔗 Patrones relacionados** - Conexiones y combinaciones

---

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Este es un proyecto educativo y valoramos:

- 📝 **Mejoras en documentación**
- 🐛 **Corrección de errores**
- 💡 **Nuevos ejemplos didácticos**
- 🌐 **Traducciones**
- 🎯 **Sugerencias pedagógicas**

Consulta [CONTRIBUTING.md](CONTRIBUTING.md) para más detalles.

---

## 📚 Recursos Adicionales

### 📖 Libros Recomendados
- "Design Patterns: Elements of Reusable Object-Oriented Software" - Gang of Four
- "Clean Architecture" - Robert C. Martin
- "Domain-Driven Design" - Eric Evans
- "Patterns of Enterprise Application Architecture" - Martin Fowler

### 🔗 Enlaces Útiles
- [Microsoft .NET Documentation](https://docs.microsoft.com/dotnet/)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp/)
- [Refactoring Guru - Design Patterns](https://refactoring.guru/design-patterns)

---

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Consulta [LICENSE](LICENSE) para más información.

---

## 🙏 Agradecimientos

Este repositorio ha sido creado con fines puramente educativos, inspirado en las mejores prácticas de la industria y la comunidad de desarrolladores. Agradecemos a todos los autores y contribuidores de los recursos que han inspirado este trabajo.

---

## 📞 Contacto

¿Preguntas? ¿Sugerencias? ¿Encontraste un error?

- 🐛 [Reportar un issue](https://github.com/tu-usuario/design-patterns-csharp/issues)
- 💬 [Iniciar una discusión](https://github.com/tu-usuario/design-patterns-csharp/discussions)

---

<div align="center">

**🎓 Aprende • 🔧 Practica • 🚀 Aplica**

*Domina los patrones de diseño y conviértete en un mejor desarrollador*

</div>
