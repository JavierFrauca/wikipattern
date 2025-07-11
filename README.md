# 🎓 Design Patterns & Architecture in C# - Educational Repository

[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/download)
[![C#](https://img.shields.io/badge/C%23-12.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

> 🌐 **Language / Idioma**: [🇺🇸 English](README.en.md) | [🇪🇸 Español](README.md) | [🇪🇸 Español (Original)](README.es.md)

---

## 📖 About This Repository

This repository is a **comprehensive educational collection** of design and architecture patterns implemented in **C# 12** with **.NET 8+**. It has been specifically created for **educational purposes** to help developers of all levels understand, learn, and apply these fundamental patterns in their professional projects.

### 🎯 Educational Objectives

- **📚 Practical Learning**: Each pattern includes realistic implementations with real-world examples
- **💡 Deep Understanding**: Detailed explanations of when, how, and why to use each pattern
- **🔧 Production-Ready Code**: Robust implementations that can serve as reference for real projects
- **🎓 Structured Progression**: From basic patterns to complex architectures
- **💼 Professional Application**: Examples contextualized in real business scenarios

### 🌟 Key Features

- ✅ **Complete Implementations**: Each pattern includes functional code and interactive demos
- ✅ **Realistic Examples**: E-commerce systems, trading, notifications, payments, etc.
- ✅ **Bilingual Documentation**: Spanish and English for greater accessibility
- ✅ **Thread-Safety**: Concurrency considerations where relevant
- ✅ **Best Practices**: Following modern C# and .NET conventions
- ✅ **Clear Use Cases**: When to use and when NOT to use each pattern
- ✅ **Difficulty Progression**: From basic concepts to advanced architectures

---

## 📋 Complete Pattern Index

### 🏗️ Creational Patterns
*Focused on flexible and reusable object creation*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**Factory Method**](src/CreationalPatterns/FactoryMethod/) | Creates objects without specifying exact class | 📧 Enterprise notification system | 🟢 Basic |
| [**Abstract Factory**](src/CreationalPatterns/AbstractFactory/) | Families of related objects | 🖥️ Cross-platform UI interfaces | 🟡 Intermediate |
| [**Builder**](src/CreationalPatterns/Builder/) | Step-by-step construction of complex objects | 🖥️ Custom PC configurator | 🟢 Basic |
| [**Singleton**](src/CreationalPatterns/Singleton/) | Single global instance | ⚙️ Application configuration and cache | 🟢 Basic |
| [**Prototype**](src/CreationalPatterns/Prototype/) | Cloning existing objects | 📄 Document templates | 🟡 Intermediate |

### 🔧 Structural Patterns
*Focused on class and object composition*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**Adapter**](src/StructuralPatterns/Adapter/) | Makes incompatible interfaces compatible | 💳 Payment system integration | 🟢 Basic |
| [**Bridge**](src/StructuralPatterns/Bridge/) | Separates abstraction from implementation | 🎮 Cross-platform media players | 🟡 Intermediate |
| [**Composite**](src/StructuralPatterns/Composite/) | Tree structure for objects | 📁 File system and directories | 🟡 Intermediate |
| [**Decorator**](src/StructuralPatterns/Decorator/) | Adds functionality dynamically | ☕ Beverage customizer | 🟢 Basic |
| [**Facade**](src/StructuralPatterns/Facade/) | Simplified interface for subsystems | 🏠 Home automation system | 🟢 Basic |
| [**Flyweight**](src/StructuralPatterns/Flyweight/) | Minimizes memory usage by sharing data | 🎮 Particle rendering engine | 🔴 Advanced |
| [**Proxy**](src/StructuralPatterns/Proxy/) | Placeholder that controls access to another object | 🖼️ Lazy image loading with cache | 🟡 Intermediate |

### 🎭 Behavioral Patterns
*Focused on algorithms and responsibility assignment*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**Chain of Responsibility**](src/BehavioralPatterns/ChainOfResponsibility/) | Chain of handlers to process requests | 🎫 Request approval system | 🟡 Intermediate |
| [**Command**](src/BehavioralPatterns/Command/) | Encapsulates requests as objects | 🎮 Editor with undo/redo | 🟡 Intermediate |
| [**Interpreter**](src/BehavioralPatterns/Interpreter/) | Interprets languages or expressions | 🧮 Mathematical expression calculator | 🔴 Advanced |
| [**Iterator**](src/BehavioralPatterns/Iterator/) | Sequential access to collection elements | 📊 Data structure traversal | 🟢 Basic |
| [**Mediator**](src/BehavioralPatterns/Mediator/) | Defines communication between objects | 💬 Multi-user chat system | 🟡 Intermediate |
| [**Memento**](src/BehavioralPatterns/Memento/) | Captures and restores object states | 🎮 Game save system | 🟡 Intermediate |
| [**Observer**](src/BehavioralPatterns/Observer/) | Notifies changes to multiple observers | 📈 Real-time trading system | 🟢 Basic |
| [**State**](src/BehavioralPatterns/State/) | Alters behavior based on internal state | 🚦 Order state machine | 🟡 Intermediate |
| [**Strategy**](src/BehavioralPatterns/Strategy/) | Family of interchangeable algorithms | 💰 Dynamic pricing system | 🟢 Basic |
| [**Template Method**](src/BehavioralPatterns/TemplateMethod/) | Defines algorithm skeleton in base class | 🍳 Customizable cooking recipes | 🟢 Basic |
| [**Visitor**](src/BehavioralPatterns/Visitor/) | Operations on object structure | 📊 Source code analyzer | 🔴 Advanced |

### 🏛️ Architectural Patterns
*Focused on overall application structure*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**MVC**](src/ArchitecturalPatterns/MVC/) | Model-View-Controller | 🌐 Web management application | 🟡 Intermediate |
| [**MVVM**](src/ArchitecturalPatterns/MVVM/) | Model-View-ViewModel | 📱 Desktop application with bindings | 🟡 Intermediate |
| [**CQRS**](src/ArchitecturalPatterns/CQRS/) | Command Query Responsibility Segregation | 🛒 E-commerce system | 🔴 Advanced |
| [**Event Sourcing**](src/ArchitecturalPatterns/EventSourcing/) | Stores events instead of state | 🏦 Banking system with audit | 🔴 Advanced |
| [**Repository**](src/ArchitecturalPatterns/Repository/) | Abstracts data access | 🗄️ Enterprise data manager | 🟡 Intermediate |
| [**Unit of Work**](src/ArchitecturalPatterns/UnitOfWork/) | Maintains list of objects affected by transaction | 💾 Database transactions | 🟡 Intermediate |
| [**Dependency Injection**](src/ArchitecturalPatterns/DependencyInjection/) | Inversion of control for dependencies | 🔧 Custom injection framework | 🟡 Intermediate |
| [**Hexagonal**](src/ArchitecturalPatterns/Hexagonal/) | Ports and adapters architecture | 🏗️ Modular enterprise system | 🔴 Advanced |

### 🎯 Domain Patterns (DDD)
*Focused on business domain modeling*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**Domain Model**](src/DomainPatterns/DomainModel/) | Rich business domain model | 🏢 Enterprise management system | 🟡 Intermediate |
| [**Entity**](src/DomainPatterns/Entity/) | Objects with unique identity | 👤 User and profile management | 🟢 Basic |
| [**Value Object**](src/DomainPatterns/ValueObject/) | Immutable objects without identity | 💰 Money, Email, Address | 🟢 Basic |
| [**Aggregate**](src/DomainPatterns/Aggregate/) | Set of entities treated as a unit | 🛒 Complete shopping cart | 🟡 Intermediate |
| [**Domain Service**](src/DomainPatterns/DomainService/) | Business logic that doesn't belong to entities | 🧮 Tax calculator | 🟡 Intermediate |
| [**Application Service**](src/DomainPatterns/ApplicationService/) | Orchestrates application use cases | 🎯 Business process coordinator | 🟡 Intermediate |
| [**Domain Events**](src/DomainPatterns/DomainEvents/) | Significant domain events | 📢 Enterprise event system | 🔴 Advanced |
| [**Specification**](src/DomainPatterns/Specification/) | Encapsulates business rules | ✅ Complex rule validator | 🟡 Intermediate |

### 🛡️ Resilience Patterns
*Focused on fault tolerance and stability*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**Circuit Breaker**](src/ResiliencePatterns/CircuitBreaker/) | Prevents cascading failures | 💳 Payment service protection | 🟡 Intermediate |
| [**Retry**](src/ResiliencePatterns/Retry/) | Retries failed operations | 🔄 Resilient HTTP client | 🟢 Basic |
| [**Bulkhead**](src/ResiliencePatterns/Bulkhead/) | Isolates critical resources | 🚢 Connection pool isolation | 🔴 Advanced |
| [**Timeout**](src/ResiliencePatterns/Timeout/) | Prevents hanging operations | ⏰ Configurable timeout client | 🟢 Basic |
| [**Rate Limiting**](src/ResiliencePatterns/RateLimiting/) | Controls request rate | 🚦 API with rate limits | 🟡 Intermediate |
| [**Fallback**](src/ResiliencePatterns/Fallback/) | Alternative response on failure | 🔄 Service with local backup | 🟡 Intermediate |

### ⚡ Concurrency Patterns
*Focused on concurrent and parallel programming*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**Producer-Consumer**](src/ConcurrencyPatterns/ProducerConsumer/) | Producers and consumers with queue | 📦 Order processing system | 🟡 Intermediate |
| [**Thread Pool**](src/ConcurrencyPatterns/ThreadPool/) | Reusable thread pool | 🔄 Parallel task processor | 🟡 Intermediate |
| [**Async-Await**](src/ConcurrencyPatterns/AsyncAwait/) | Modern asynchronous programming | 🛒 Async order processing | 🟡 Intermediate |
| [**Pipeline**](src/ConcurrencyPatterns/Pipeline/) | Parallel stage processing | 🏭 Data processing pipeline | 🔴 Advanced |

### 🧪 Testing Patterns
*Focused on testing and code quality*

| Pattern | Description | Educational Example | Level |
|---------|-------------|-------------------|-------|
| [**TDD**](src/TestingPatterns/TDD/) | Test-Driven Development | ✅ Test-driven development | 🟢 Basic |
| [**BDD**](src/TestingPatterns/BDD/) | Behavior-Driven Development | 📋 Executable specifications | 🟡 Intermediate |
| [**AAA Pattern**](src/TestingPatterns/AAA/) | Arrange-Act-Assert | 🎯 Unit test structure | 🟢 Basic |
| [**Test Doubles**](src/TestingPatterns/TestDoubles/) | Mocks, Stubs, Fakes | 🎭 Test doubles for isolation | 🟡 Intermediate |
| [**Page Object Model**](src/TestingPatterns/PageObjectModel/) | Web page abstraction for testing | 🌐 UI test automation | 🟡 Intermediate |

---

## 🚀 Getting Started

### 📋 Prerequisites

- **.NET 8.0 SDK** or higher
- **C# 12** compatible IDE (Visual Studio 2022, VS Code, Rider)
- **Git** to clone the repository

### 📥 Installation

```bash
# Clone the repository
git clone https://github.com/your-username/design-patterns-csharp.git

# Navigate to directory
cd design-patterns-csharp

# Restore packages (if needed)
dotnet restore
```

### 🎯 How to Use This Repository

#### 📚 For Students

1. **Start with basic patterns** (🟢) like Factory Method, Observer, Strategy
2. **Read the documentation** for each pattern before viewing the code
3. **Run the demos** to see patterns in action
4. **Experiment by modifying** examples to better understand
5. **Progress gradually** towards more complex patterns

#### 👨‍🏫 For Instructors

- Each pattern includes **complete educational material**
- **Progressive examples** from basic concepts to complex implementations
- **Real use cases** that students can relate to
- **Implicit exercises** through variations in demos

#### 💼 For Professionals

- **Quick reference** for production-ready implementations
- **Best practices** applied in each example
- **Performance considerations** and thread-safety
- **Reusable code** for real projects

### 🎮 Running the Examples

Each pattern includes interactive demos:

```csharp
// Example: Factory Method
NotificationDemo.RunDemo();

// Example: Observer Pattern  
await StockTradingDemo.RunDemo();

// Example: Strategy Pattern
StrategyPatternDemo.RunDemo();

// Example: Async/Await
await AsyncAwaitPatternDemo.RunDemo();
```

---

## 🏆 Learning Methodology

### 📈 Suggested Progression

1. **🟢 Basic Level (1-2 weeks)**
   - Factory Method, Singleton, Strategy, Observer
   - Template Method, Iterator, Decorator

2. **🟡 Intermediate Level (2-3 weeks)**
   - Builder, Adapter, Command, State
   - Repository, Unit of Work, MVC/MVVM

3. **🔴 Advanced Level (3-4 weeks)**
   - CQRS, Event Sourcing, Hexagonal Architecture
   - Circuit Breaker, Pipeline, Domain Events

### 🎯 Educational Approach

Each pattern follows a consistent educational structure:

1. **📖 Conceptual introduction** - What problem does it solve?
2. **🎯 When to use it** - Appropriate scenarios
3. **⚠️ When NOT to use it** - Anti-patterns and warnings
4. **💡 Step-by-step implementation** - Gradual construction
5. **🔧 Realistic example** - Real-world use case
6. **✅ Interactive demo** - Executable code
7. **📊 Results analysis** - Lessons learned
8. **🔗 Related patterns** - Connections and combinations

---

## 🤝 Contributing

Contributions are welcome! This is an educational project and we value:

- 📝 **Documentation improvements**
- 🐛 **Bug fixes**
- 💡 **New educational examples**
- 🌐 **Translations**
- 🎯 **Pedagogical suggestions**

See [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

---

## 📚 Additional Resources

### 📖 Recommended Books
- "Design Patterns: Elements of Reusable Object-Oriented Software" - Gang of Four
- "Clean Architecture" - Robert C. Martin
- "Domain-Driven Design" - Eric Evans
- "Patterns of Enterprise Application Architecture" - Martin Fowler

### 🔗 Useful Links
- [Microsoft .NET Documentation](https://docs.microsoft.com/dotnet/)
- [C# Programming Guide](https://docs.microsoft.com/dotnet/csharp/)
- [Refactoring Guru - Design Patterns](https://refactoring.guru/design-patterns)

---

## 📄 License

This project is under the MIT License. See [LICENSE](LICENSE) for more information.

---

## 🙏 Acknowledgments

This repository has been created for purely educational purposes, inspired by industry best practices and the developer community. We thank all authors and contributors of the resources that have inspired this work.

---

## 📞 Contact

Questions? Suggestions? Found a bug?

- 🐛 [Report an issue](https://github.com/your-username/design-patterns-csharp/issues)
- 💬 [Start a discussion](https://github.com/your-username/design-patterns-csharp/discussions)
- 📧 Email: your-email@example.com

---

<div align="center">

**🎓 Learn • 🔧 Practice • 🚀 Apply**

*Master design patterns and become a better developer*

</div>
