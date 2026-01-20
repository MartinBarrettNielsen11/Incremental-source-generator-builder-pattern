
namespace Generator;

/// <summary>
/// Helper class for holding properties used for code generation
/// </summary>
internal readonly struct Properties(
    EquatableArray<PropertyInfoModel> normal,
    EquatableArray<PropertyInfoModel> collection)
{
    internal EquatableArray<PropertyInfoModel> Normal { get; } = normal;
    internal EquatableArray<PropertyInfoModel> Collection { get; } = collection;

    internal List<PropertyInfoModel> AllProperties => new(Normal.Concat(Collection).ToArray());
}