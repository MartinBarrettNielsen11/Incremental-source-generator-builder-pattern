namespace Incremental_source_generator_builder_pattern;

internal struct HashCode
{
    private int _value;

    public void Add<T>(T value)
    {
        int h = value?.GetHashCode() ?? 0;
        unchecked
        {
            _value = (_value * 31) + h;
        }
    }

    public int ToHashCode() => _value;

    public static implicit operator int(HashCode hashCode) => hashCode._value;
}