using System.Collections;
using System.Collections.Immutable;

namespace Incremental_source_generator_builder_pattern;

/// <summary>
/// An immutable, equatable array. This is equivalent to <see cref="Array{T}"/> but with value equality support.
/// This is of interest to facilitate comparison
/// </summary>
/// <typeparam name="T">The type of values in the array.</typeparam>
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyCollection<T>
    where T : IEquatable<T>
{
    private readonly T[]? _array;

    /// <summary>
    /// Creates a new <see cref="EquatableArray{T}"/> instance.
    /// </summary>
    /// <param name="array">The input <see cref="ImmutableArray"/> to wrap.</param>
    public EquatableArray(T[] array)
    {
        _array = array;
    }

    public bool Equals(EquatableArray<T> array)
    { 
        // SequenceEqual(): returns true if two collections contain then same items
        // - and since the items in our collection are records, then they will automatically support value equality. 

        // Generally, the Equal method is used for built-in collections - thus breaking the caching
        return AsSpan().SequenceEqual(array.AsSpan());
    }

    /// <sinheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> array && this.Equals(array);
    }

    /// <sinheritdoc/>
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

    /// <summary>
    /// Returns a <see cref="ReadOnlySpan{T}"/> wrapping the current items.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> wrapping the current items.</returns>
    public ReadOnlySpan<T> AsSpan() => _array.AsSpan();

    /// <sinheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)(_array ?? [])).GetEnumerator();
    }    
    
    /// <sinheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<T>)(_array ?? [])).GetEnumerator();
    }

    public int Count => _array?.Length ?? 0;

    /// <summary>
    /// Checks whether two <see cref="EquatableArray{T}"/> values are the same.
    /// </summary>
    /// <param name="left">The first <see cref="EquatableArray{T}"/> value.</param>
    /// <param name="right">The second <see cref="EquatableArray{T}"/> value.</param>
    /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> are equal.</returns>
    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    /// <summary>
    /// Checks whether two <see cref="EquatableArray{T}"/> values are not the same.
    /// </summary>
    /// <param name="left">The first <see cref="EquatableArray{T}"/> value.</param>
    /// <param name="right">The second <see cref="EquatableArray{T}"/> value.</param>
    /// <returns>Whether <paramref name="left"/> and <paramref name="right"/> are not equal.</returns>
    public static bool operator != (EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}