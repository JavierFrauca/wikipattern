using System;

// Bulkhead Pattern (aislamiento de recursos)
public class Bulkhead
{
    private int _maxConcurrent;
    private int _current;
    public Bulkhead(int maxConcurrent) => _maxConcurrent = maxConcurrent;
    public bool TryEnter()
    {
        if (_current < _maxConcurrent)
        {
            _current++;
            return true;
        }
        return false;
    }
    public void Leave() => _current--;
}

// Ejemplo de uso
// var bulkhead = new Bulkhead(2);
// if (bulkhead.TryEnter()) { /* hacer trabajo */ bulkhead.Leave(); }
