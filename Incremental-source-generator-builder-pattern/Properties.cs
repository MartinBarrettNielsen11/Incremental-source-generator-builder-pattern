namespace Incremental_source_generator_builder_pattern;

/// <summary>
/// Helper class for holding properties used for code generation
/// </summary>
internal readonly struct Properties(
    List<PropertyInfoModel> normal,
    List<PropertyInfoModel> collection)
{
    public List<PropertyInfoModel> Normal { get; } = normal;
    public List<PropertyInfoModel> Collection { get; } = collection;

    public List<PropertyInfoModel> AllProperties => new(Normal.Concat(Collection).ToArray());
}