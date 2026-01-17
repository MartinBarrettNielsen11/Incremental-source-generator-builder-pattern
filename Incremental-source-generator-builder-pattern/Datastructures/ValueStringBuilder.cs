using System.Runtime.CompilerServices;

namespace Incremental_source_generator_builder_pattern;

internal ref struct ValueStringBuilder(Span<char> initialBuffer)
{
    private char[]? _arrayToReturnToPool = null;
    private Span<char> _chars = initialBuffer;
    private int _pos = 0;
    
    internal void Append(ReadOnlySpan<char> value)
    {
        var pos = _pos;
        if (pos > _chars.Length - value.Length)
            Grow(capacity: value.Length);

        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    
    /// <summary>
    /// Resize the internal buffer
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int capacity)
    {
        // double the buffer each time unless the needed input is even larger
        var minimumCapacity = (int)Math.Max((uint)(_pos + capacity), (uint)_chars.Length * 2);
        
        char[] poolArray = ArrayPool<char>.Shared.Rent(minimumCapacity);
        _chars.Slice(0, _pos).CopyTo(poolArray);

        char[]? toReturn = _arrayToReturnToPool;
        _chars = _arrayToReturnToPool = poolArray;
        if (toReturn is not null)
            ArrayPool<char>.Shared.Return(toReturn);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Append(char ch)
    {
        var pos = _pos;
        if ((uint)pos >= (uint)_chars.Length)
            GrowAndAppend(ch);
        else
        {
            _chars[pos] = ch;
            _pos = pos + 1;
        }
    }
    
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char ch)
    {
        Grow(capacity: 1);
        Append(ch);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Dispose()
    {
        char[]? toReturn = _arrayToReturnToPool;
        this = default;
        if (toReturn is not null)
            ArrayPool<char>.Shared.Return(toReturn);
    }
    
    public override string ToString()
    {
        var s = _chars.Slice(0, _pos).ToString();
        Dispose();
        return s;
    }
}