using System;

public abstract class LCG
{
    protected readonly int _multiplier;
    protected readonly int _modulus;

    protected long _startingSeed;
    protected long _currentSeed;

    public LCG(int multiplier, int modulus, long? seed = null)
    {
        _multiplier = multiplier;
        _modulus = modulus;

        if (seed == null)
            seed = DateTime.Now.Ticks;

        _startingSeed = (long)seed;

        _currentSeed = _startingSeed;
    }

    public void SetSeed(long seed)
    {
        _startingSeed = seed;
        _currentSeed = seed;
    }

    public int Next()
    {
        _currentSeed = (_currentSeed * _multiplier) % _modulus;
        return (int)_currentSeed;
    }

    public float NextFloat()
    {
        return (float)Next() / (_modulus - 1);
    }

    public int Range(int min, int max)
    {
        if (min > max)
            return int.MinValue;

        return (Next() % (max - min)) + min;
    }

    public float Range(float min, float max)
    {
        if (min > max)
            return float.MinValue;

        return (NextFloat() * (max - min)) + min;
    }

    public int Sign()
    {
        if (NextFloat() < 0.5f)
            return 1;
        else
            return -1;
    }
}

public class MillerParkLCG : LCG
{
    public MillerParkLCG()
        : base(16807, 2147483647)
    {
    }

    public MillerParkLCG(long seed)
        : base(16807, 2147483647, seed)
    {
    }
}