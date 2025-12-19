using Microsoft.CodeAnalysis;

namespace Incremental_source_generator_builder_pattern.Contracts;

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
    
    