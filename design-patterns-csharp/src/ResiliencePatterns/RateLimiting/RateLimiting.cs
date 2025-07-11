using System;
using System.Threading;

// Rate Limiting Pattern
public class RateLimiter
{
    private int _count = 0;
    private int _limit;
    public RateLimiter(int limit) => _limit = limit;
    public bool Allow()
    {
        if (_count < _limit)
        {
            _count++;
            return true;
        }
        return false;
    }
    public void Reset() => _count = 0;
}

// Ejemplo de uso
// var limiter = new RateLimiter(2);
// Console.WriteLine(limiter.Allow()); // Output: True
// Console.WriteLine(limiter.Allow()); // Output: True
// Console.WriteLine(limiter.Allow()); // Output: False
