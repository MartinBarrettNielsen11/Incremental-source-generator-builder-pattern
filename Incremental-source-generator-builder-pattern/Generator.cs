using Incremental_source_generator_builder_pattern;
using Microsoft.CodeAnalysis.Text;

namespace Incremental_source_generator_builder_pattern;

/// <summary>
/// Incremental source generator for creating Builder-classes.
/// </summary>
#pragma warning disable RS1038
[Generator(LanguageNames.CSharp)]
#pragma warning restore RS1038
public sealed class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                $"{Constants.BuilderAttributeName}.g.cs", 
                SourceText.From(BuilderSourceEmitter.GenerateBuilderAttribute(Constants.BuilderAttributeName), Encoding.UTF8));

            context.AddSource(
                $"{Constants.DomainAssertionExtensions}.g.cs", 
                SourceText.From(BuilderSourceEmitter.GenerateDomainAssertionExtensions(typeof(Generator).Namespace!), Encoding.UTF8));
        });
        
        IncrementalValuesProvider<BuilderToGenerate> builders = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                $"{typeof(BuilderToGenerate).Namespace}.{Constants.BuilderAttributeName}",
                // Performs a first-pass filtering of syntax nodes that could possibly represent a builder class
                // we just return true - as it's guaranteed to always be a node decorated with that attribute!
                
                // Apply global predicate - as it is guarenteed to always be a node decorated with the Build attribute
                predicate: static (_, _) => true,
                transform: static (generatorAttributeSyntaxContext, ct) => 
                    GetTypeToGenerate(generatorAttributeSyntaxContext, ct))
            .WithTrackingName(TrackingNames.InitialExtraction)
            .Where(static b => b is not null)
            .Select(static (b, _) => b!.Value)
            .WithTrackingName(TrackingNames.RemovingNulls);

        context.RegisterSourceOutput(builders, static (sourceProductionContext, builder) =>
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
    
    // Transform the syntax context
    private static BuilderToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext syntaxContext, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (syntaxContext.TargetSymbol is not INamedTypeSymbol builderSymbol)
            return null;

        // Get the target type from the attribute [BuilderFor(typeof(Entity))]
        AttributeData? attribute = syntaxContext.Attributes.FirstOrDefault();
        
        if (attribute?.ConstructorArguments.Length is not > 0)
            return null;

        if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol targetType)
            return null;

        // short-circuit the transformation if additional changes are detected
        ct.ThrowIfCancellationRequested();

        INamedTypeSymbol? typeICollection = syntaxContext.SemanticModel.Compilation
            .GetTypeByMetadataName(typeof(ICollection<>).FullName!);

        Properties properties = Helpers.GetPropertySymbols(typeICollection!, targetType);

        return new BuilderToGenerate(
            BuilderClassName: builderSymbol.Name,
            BuilderClassNamespace: builderSymbol.ContainingNamespace.ToString(),
            Properties: properties,
            TargetClassFullName: targetType.ToString(),
            ElapsedTime: stopwatch.Elapsed);
    }
}
