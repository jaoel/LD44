using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// How to use this class:
// Create a new serializable class that inherits from ProbabilityList<T>
// [System.Serializable]
// public class GameObjectProbabilityList : ProbabilityList<GameObject> { }
// Use this new class as you would a regular list.
// Now you have the power of random in your hands!

public abstract class ProbabilityListBase
{
    public abstract System.Type GetTemplateType();
}

public class ProbabilityListElement<T>
{
    public T value;
    public int priority;
}

[System.Serializable]
public class ProbabilityList<T> : ProbabilityListBase, IEnumerable<ProbabilityListElement<T>>
{
    [SerializeField] private List<T> _values = new List<T>();
    [SerializeField] private List<int> _priorities = new List<int>();
    [SerializeField] private int _totalPriority = 0;

    public int Count => _values.Count;
    public T[] Values { get { return _values.ToArray(); } }

    public override System.Type GetTemplateType()
    {
        return typeof(T);
    }

    public void Add(T value, int priority)
    {
        _values.Add(value);
        _priorities.Add(priority);
        _totalPriority += priority;
    }

    public void Remove(T value)
    {
        int index = _values.IndexOf(value);
        _totalPriority -= _priorities[index];
        _values.RemoveAt(index);
        _priorities.RemoveAt(index);
    }

    public T GetRandom()
    {
        int random = Random.Range(0, _totalPriority) + 1;
        int acc = 0;
        for (int i = 0; i < _values.Count; i++)
        {
            (T value, int priority) = Get(i);
            if (random > acc && random <= (acc + priority))
            {
                return value;
            }
            acc += priority;
        }
        return default;
    }

    public T GetRandom(System.Func<T, bool> filter)
    {
        int filteredTotal = 0;
        List<int> filteredIndices = new List<int>();
        if (filter != null)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                (T value, int priority) = Get(i);
                if (filter(value))
                {
                    filteredTotal += priority;
                    filteredIndices.Add(i);
                }
            }
        }

        int random = Random.Range(0, filteredTotal) + 1;
        int acc = 0;
        for (int f = 0; f < filteredIndices.Count; f++)
        {
            int i = filteredIndices[f];
            (T value, int priority) = Get(i);
            if (random > acc && random <= (acc + priority))
            {
                return value;
            }
            acc += priority;
        }
        return default;
    }

    private (T, int) Get(int index)
    {
        if (_values.Count > index)
        {
            return (_values[index], _priorities[index]);
        }
        return default;
    }

    public bool Contains(T value)
    {
        for (int i = 0; i < _values.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_values[i], value))
            {
                return true;
            }
        }
        return false;
    }

    public void Clear()
    {
        _values.Clear();
        _priorities.Clear();
        _totalPriority = 0;
    }

    public float GetProbability(T item)
    {
        if (_totalPriority > 0)
        {
            for (int i = 0; i < _values.Count; i++)
            {
                T value = _values[i];
                int priority = _priorities[i];
                if (EqualityComparer<T>.Default.Equals(item, value))
                {
                    return (float)priority / _totalPriority;
                }
            }
        }
        return 0.0f;
    }

    public T GetValue(int index)
    {
        return (_values[index]);
    }

    public IEnumerator<ProbabilityListElement<T>> GetEnumerator()
    {
        for (int i = 0; i < _values.Count; i++)
        {
            yield return new ProbabilityListElement<T> { value = _values[i], priority = _priorities[i] };
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Overlay(ProbabilityList<T> other)
    {
        for (int i = 0; i < other._values.Count; i++)
        {
            T overlayValue = other._values[i];
            for (int k = 0; k < _values.Count; i++)
            {
                T value = _values[k];

                if (overlayValue.Equals(value))
                {
                    _totalPriority -= _priorities[k];
                    _priorities[k] = other._priorities[i];
                    _totalPriority += _priorities[k];
                }
            }
        }
    }

    public void CopyFrom(ProbabilityList<T> other)
    {
        _values = new List<T>(other._values);
        _priorities = new List<int>(other._priorities);
        _totalPriority = other._totalPriority;
    }
}
