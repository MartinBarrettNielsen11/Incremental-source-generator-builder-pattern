namespace Incremental_source_generator_builder_pattern;

/// <summary>
/// Names that are attached to incremental generator stages for tracking
/// </summary>
internal static class TrackingNames
{
    public const string InitialExtraction = nameof(InitialExtraction);
    public const string RemovingNulls = nameof(RemovingNulls);
}
