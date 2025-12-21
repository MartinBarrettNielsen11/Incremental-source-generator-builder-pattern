using System.Diagnostics;
using System.Text;
using Incremental_source_generator_builder_pattern.Contracts;
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
        IncrementalValuesProvider<BuilderToGenerate> res = context.SyntaxProvider.ForAttributeWithMetadataName(
                $"{typeof(BuilderToGenerate).Namespace}.{Constants.BuilderAttributeName}",
                predicate: static (_, _) => true,
                transform: static (generatorAttributeSyntaxContext, ct) =>
                    Transform(generatorAttributeSyntaxContext, ct))
            .Where(static b => b is not null)
            .Select(static (b, _) => b!.Value);

        context.RegisterSourceOutput(res, static (sourceProductionContext, builder) =>
        {
            try
            {
                var generatedOutput = BuilderSourceEmitter.GenerateBuilder(builder);

                // move extensions addition down here - and make tests for it
                sourceProductionContext.AddSource(
                    $"{builder.BuilderClassNamespace.Replace(".", "_")}_{builder.BuilderClassName}.cs",
                    SourceText.From(generatedOutput, Encoding.UTF8)
                );
            }
            catch (Exception e)
            {
                sourceProductionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "BGN001",
                            "Unexpected error", 
                            $"An error occurred while generating a builder for '{builder.TargetClassFullName}'\n{e.Message}", 
                            "BuilderGenerator", DiagnosticSeverity.Error, true), null));   
            }
        });
    }

    private static BuilderToGenerate? Transform(GeneratorAttributeSyntaxContext sc, CancellationToken ct)
    {
        var stopWatch = Stopwatch.StartNew();
        
        if (sc.TargetSymbol is not INamedTypeSymbol builderSymbol)
            return null;

        // Get the target type from the attribute [BuilderFor(typeof(Entity))]
        AttributeData? attribute = sc.Attributes.FirstOrDefault();
        
        if (attribute?.ConstructorArguments.Length is not > 0)
            return null;

        if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol targetType)
            return null;

        // short-circuit the transformation if additional changes are detected
        ct.ThrowIfCancellationRequested();
        
        return new BuilderToGenerate(
            BuilderClassName: "test",
            BuilderClassNamespace: "test",
            Properties: new  Properties(),
            TargetClassFullName: "test",
            ElapsedTime: stopWatch.Elapsed);
    }
}