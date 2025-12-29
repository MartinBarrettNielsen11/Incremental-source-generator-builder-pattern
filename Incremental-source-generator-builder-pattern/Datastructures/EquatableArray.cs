using System.Collections;

namespace Incremental_source_generator_builder_pattern;

public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyCollection<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;
    
    public EquatableArray(T[] array)
    {
        _array = array;
    }

    public bool Equals(EquatableArray<T> array)
    { 
        return AsSpan().SequenceEqual(array.AsSpan());
    }
    
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> array && this.Equals(array);
    }
    
    public override int GetHashCode()
    {
        if (_array is not T[] array)
            return 0;

        HashCode hashCode = default;

        foreach (T item in array)
        {
            hashCode.Add(item);
        }

        return hashCode.ToHashCode();
    }
    
    public ReadOnlySpan<T> AsSpan() => _array.AsSpan();
    
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)(_array ?? [])).GetEnumerator();
    }    
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<T>)(_array ?? [])).GetEnumerator();
    }

    public int Count => _array?.Length ?? 0;
    
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    
    public static bool operator != (EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}