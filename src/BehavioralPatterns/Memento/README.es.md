# Memento Pattern - Patrón Recuerdo

## 📋 Descripción

El **Patrón Memento** permite capturar y externalizar el estado interno de un objeto sin violar la encapsulación, de manera que el objeto pueda ser restaurado a este estado más tarde.

## 🎯 Propósito

- **Preservar estado**: Guardar snapshots del estado de un objeto
- **Undo/Redo**: Implementar funcionalidades de deshacer y rehacer
- **Rollback**: Volver a un estado anterior cuando algo falla
- **Checkpoints**: Crear puntos de control en procesos largos

## ✅ Cuándo Usarlo

- **Editores**: Texto, imagen, código con funcionalidad undo/redo
- **Juegos**: Sistema de guardado y carga de partidas
- **Transacciones**: Rollback en caso de error
- **Wizards**: Navegación hacia atrás en formularios multi-paso
- **Debugging**: Guardar estados para análisis

## ❌ Cuándo NO Usarlo

- **Estados pequeños**: Cuando el objeto tiene poco estado
- **Estados inmutables**: Objetos que no cambian
- **Memoria limitada**: Cuando guardar estados consume demasiada memoria
- **Estados sensibles**: Información que no debe persistir por seguridad

## 🏗️ Estructura

```
Originador ←→ Memento
    ↓           ↑
Caretaker ------+
```

### Participantes

1. **Originador**: Objeto cuyo estado queremos guardar
2. **Memento**: Almacena el estado del originador
3. **Caretaker**: Gestiona los mementos (cuándo guardar/restaurar)

## 💡 Implementación

```csharp
// Memento - Almacena el estado
public class DocumentMemento
{
    public string Content { get; }
    public int CursorPosition { get; }
    public DateTime Timestamp { get; }
    
    internal DocumentMemento(string content, int cursorPosition)
    {
        Content = content;
        CursorPosition = cursorPosition;
        Timestamp = DateTime.UtcNow;
    }
}

// Originador - Objeto cuyo estado queremos guardar
public class Document
{
    public string Content { get; private set; } = "";
    public int CursorPosition { get; private set; } = 0;
    
    public void Write(string text)
    {
        Content = Content.Insert(CursorPosition, text);
        CursorPosition += text.Length;
    }
    
    public DocumentMemento CreateMemento()
    {
        return new DocumentMemento(Content, CursorPosition);
    }
    
    public void RestoreFromMemento(DocumentMemento memento)
    {
        Content = memento.Content;
        CursorPosition = memento.CursorPosition;
    }
}

// Caretaker - Gestiona los mementos
public class DocumentHistory
{
    private readonly Stack<DocumentMemento> _history = new();
    private readonly Stack<DocumentMemento> _redoStack = new();
    
    public void SaveState(Document document)
    {
        _history.Push(document.CreateMemento());
        _redoStack.Clear(); // Limpiar redo al hacer nueva acción
    }
    
    public void Undo(Document document)
    {
        if (_history.Count > 0)
        {
            var currentState = document.CreateMemento();
            _redoStack.Push(currentState);
            
            var previousState = _history.Pop();
            document.RestoreFromMemento(previousState);
        }
    }
    
    public void Redo(Document document)
    {
        if (_redoStack.Count > 0)
        {
            var currentState = document.CreateMemento();
            _history.Push(currentState);
            
            var redoState = _redoStack.Pop();
            document.RestoreFromMemento(redoState);
        }
    }
}
```

## 📊 Ejemplo de Uso

```csharp
public class TextEditorDemo
{
    public static void RunDemo()
    {
        var document = new Document();
        var history = new DocumentHistory();
        
        // Escribir texto
        document.Write("Hola ");
        history.SaveState(document); // Checkpoint 1
        
        document.Write("mundo");
        history.SaveState(document); // Checkpoint 2
        
        document.Write("!");
        Console.WriteLine($"Contenido: '{document.Content}'"); // "Hola mundo!"
        
        // Deshacer
        history.Undo(document);
        Console.WriteLine($"Después de undo: '{document.Content}'"); // "Hola mundo"
        
        // Rehacer
        history.Redo(document);
        Console.WriteLine($"Después de redo: '{document.Content}'"); // "Hola mundo!"
    }
}
```

## 🔧 Variaciones del Patrón

### 1. **Memento con Compresión**

```csharp
public class CompressedMemento
{
    private readonly byte[] _compressedData;
    
    public CompressedMemento(object state)
    {
        var json = JsonSerializer.Serialize(state);
        _compressedData = GZip.Compress(Encoding.UTF8.GetBytes(json));
    }
    
    public T Restore<T>()
    {
        var decompressed = GZip.Decompress(_compressedData);
        var json = Encoding.UTF8.GetString(decompressed);
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

### 2. **Memento Incremental**

```csharp
public class IncrementalMemento
{
    public object ChangedProperties { get; }
    public DateTime Timestamp { get; }
    
    // Solo guarda las propiedades que cambiaron
    public IncrementalMemento(object changes)
    {
        ChangedProperties = changes;
        Timestamp = DateTime.UtcNow;
    }
}
```

### 3. **Memento Persistente**

```csharp
public class PersistentCaretaker
{
    private readonly string _filePath;
    
    public async Task SaveMementoAsync(IMemento memento)
    {
        var json = JsonSerializer.Serialize(memento);
        await File.WriteAllTextAsync(_filePath, json);
    }
    
    public async Task<T> LoadMementoAsync<T>() where T : IMemento
    {
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

## 🎮 Ejemplo: Sistema de Guardado de Juego

```csharp
public class GameState
{
    public int Level { get; set; }
    public int Score { get; set; }
    public int Lives { get; set; }
    public Vector3 PlayerPosition { get; set; }
    public List<string> Inventory { get; set; } = new();
}

public class Game
{
    private GameState _state = new();
    
    public GameMemento CreateSavePoint()
    {
        return new GameMemento(_state);
    }
    
    public void LoadFromSave(GameMemento save)
    {
        _state = save.GetGameState();
    }
}

public class SaveManager
{
    private readonly Dictionary<string, GameMemento> _saves = new();
    
    public void QuickSave(Game game, string slotName = "quicksave")
    {
        _saves[slotName] = game.CreateSavePoint();
    }
    
    public void QuickLoad(Game game, string slotName = "quicksave")
    {
        if (_saves.TryGetValue(slotName, out var save))
        {
            game.LoadFromSave(save);
        }
    }
}
```

## ⚡ Consideraciones de Rendimiento

- **Memoria**: Los mementos pueden consumir mucha memoria
- **Frecuencia**: No guardar en cada pequeño cambio
- **Límite de historia**: Mantener solo N mementos recientes
- **Compresión**: Comprimir mementos grandes
- **Lazy loading**: Cargar mementos solo cuando se necesiten

## 🧪 Testing

```csharp
[Test]
public void Memento_ShouldRestoreExactState()
{
    // Arrange
    var document = new Document();
    document.Write("Test content");
    document.SetCursorPosition(5);
    
    // Act
    var memento = document.CreateMemento();
    document.Write(" more text");
    document.RestoreFromMemento(memento);
    
    // Assert
    Assert.AreEqual("Test content", document.Content);
    Assert.AreEqual(5, document.CursorPosition);
}

[Test]
public void History_ShouldSupportMultipleUndoRedo()
{
    // Arrange
    var document = new Document();
    var history = new DocumentHistory();
    
    // Act
    document.Write("A");
    history.SaveState(document);
    
    document.Write("B");
    history.SaveState(document);
    
    document.Write("C");
    
    // Multiple undo
    history.Undo(document);
    history.Undo(document);
    
    // Assert
    Assert.AreEqual("A", document.Content);
    
    // Redo
    history.Redo(document);
    Assert.AreEqual("AB", document.Content);
}
```

## 📊 Métricas y Optimización

```csharp
public class MementoMetrics
{
    public int TotalMementos { get; private set; }
    public long TotalMemoryUsed { get; private set; }
    public TimeSpan AverageRestoreTime { get; private set; }
    
    public void RecordMemento(IMemento memento)
    {
        TotalMementos++;
        TotalMemoryUsed += GetMementoSize(memento);
    }
}
```

## 🔗 Patrones Relacionados

- **[Command](../../BehavioralPatterns/Command/)**: Puede usar Memento para undo/redo
- **[Prototype](../../CreationalPatterns/Prototype/)**: Clonación de objetos vs guardado de estado
- **[State](../../BehavioralPatterns/State/)**: Ambos manejan estados, pero con diferentes propósitos

## 📚 Recursos Adicionales

- [Gang of Four: Memento Pattern](https://en.wikipedia.org/wiki/Memento_pattern)
- [Refactoring Guru: Memento](https://refactoring.guru/design-patterns/memento)
- [.NET Memory Management](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)

---

> 💡 **Tip**: Considera usar weak references o límites de memoria para evitar memory leaks cuando manejes muchos mementos.
