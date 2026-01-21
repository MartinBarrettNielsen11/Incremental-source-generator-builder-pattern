using System.Collections;

namespace Generator;

/// <summary>
/// An immutable, equatable array. This is intended for facilitating comparison
/// </summary>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyCollection<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;
    
    public EquatableArray(T[] array) => _array = array;

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

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)(_array ?? [])).GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)(_array ?? [])).GetEnumerator();

    public int Count => _array?.Length ?? 0;
    
    public static bool operator == (EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    
    public static bool operator != (EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
    
    public override bool Equals(object? obj) => obj is EquatableArray<T> array && Equals(array);
    
    /*  Uses SequenceEqual - which returns true if two collections contain the same items.
     *  And - as the items in our collection are records - then they will automatically support value equality
     *  Generally, the Equal() method is used for built-in collection - thus breaking the caching
     */ 
    public bool Equals(EquatableArray<T> array) => AsSpan().SequenceEqual(array.AsSpan());
    
    private ReadOnlySpan<T> AsSpan() => _array.AsSpan();
}