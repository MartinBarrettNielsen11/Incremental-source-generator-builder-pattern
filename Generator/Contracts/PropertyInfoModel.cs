namespace Generator;

/// <summary>
/// Immutable, value-equatable subset of IPropertySymbol data.
/// Safe for use in incremental generator pipelines.
/// </summary>
internal record struct PropertyInfoModel(
    string Name,
    string TypeName,
    bool IsCollection,
    bool HasSetter,
    Accessibility Accessibility);
    
    