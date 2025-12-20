using System.Text;
using Incremental_source_generator_builder_pattern.Helpers;
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
                SourceText.From(BuilderSourceEmitter.GenerateBuilderAttribute(Constants.BuilderAttributeName), Encoding.UTF8));
        });    
    }
}