using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                $"{Constants.BuilderAttributeName}.g.cs", 
                SourceText.From(string.Empty, Encoding.UTF8));
        });    
    }
}