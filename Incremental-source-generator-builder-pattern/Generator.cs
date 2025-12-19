using Microsoft.CodeAnalysis;

namespace Incremental_source_generator_builder_pattern;

/// <summary>
/// Incremental source generator for creating Builder-classes.
/// </summary>

#pragma warning disable RS1038
[Generator(LanguageNames.CSharp)]
#pragma warning restore RS1038
internal sealed class BuilderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        throw new NotImplementedException();
    }
}