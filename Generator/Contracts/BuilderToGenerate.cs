using Generator;

namespace Generator;

/// <summary>
///Helper struct for transferring data to the builder.
/// containing the information needed to generate a Builder
/// </summary> // note: maybe do some readonly stuff here like Andrew Lock?
internal record struct BuilderToGenerate(
    string BuilderClassName,
    string BuilderClassNamespace,
    Properties Properties,
    string TargetClassFullName,
    TimeSpan ElapsedTime);
    
    