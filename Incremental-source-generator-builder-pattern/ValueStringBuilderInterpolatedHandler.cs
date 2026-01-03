using System.Runtime.CompilerServices;

namespace Incremental_source_generator_builder_pattern;

[InterpolatedStringHandler]
internal ref struct ValueStringBuilderInterpolatedHandler
{
    private ValueStringBuilder _vsb; // <-- ref field!

    public ValueStringBuilderInterpolatedHandler(int literalLength, int formattedCount, ValueStringBuilder vsb)
    {
        _vsb = vsb; // keep a ref, not a copy
        // optional: pre-grow if you want, e.g. _vsb.EnsureCapacity(literalLength + formattedCount * 10);
    }

    public void AppendLiteral(string value) => _vsb.Append(value);

    public void AppendFormatted<T>(T value)
    {
        if (value is null) return;
        _vsb.Append(value.ToString()!);
    }

    public void AppendFormatted<T>(T value, string format)
    {
        if (value is IFormattable f) _vsb.Append(f.ToString(format, null)!);
        else AppendFormatted(value);
    }
    
    public void Commit(ref ValueStringBuilder target)
    {
        // copy accumulated text back into caller
        target.Append(_vsb.ToStringV2());
    }
}