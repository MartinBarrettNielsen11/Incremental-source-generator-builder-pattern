using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GeneratorTests;

// Pertains to the incremental source generator pipeline, which memoizes results
// at each transform/filter stage to avoid redundant work when inputs don’t change.
public class CachingTests
{
    [Test]
    public async Task IncrementalCachingMechanismIsUsedBetweenRuns()
    {
        var input = await TestHelpers.GetSourceText(TestConstants.Example1);
        var stages = TestHelpers.GetTrackingNames(typeof(TrackingNames));
        
        (ImmutableArray<Diagnostic> diagnostics, var output) = await GetGeneratedTrees<Generator.Generator>([input], stages);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
        await Assert.That(output.Length).IsGreaterThan(0);
    }


    private static async Task<(ImmutableArray<Diagnostic> diagnostics, string[] Output)> GetGeneratedTrees<T>(
        string[] sources, 
        string[] stages) 
        where T : IIncrementalGenerator, new()
    {
        IEnumerable<SyntaxTree> syntaxTrees = sources.Select(static s => CSharpSyntaxTree.ParseText(s));

        IEnumerable<PortableExecutableReference> references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Concat([MetadataReference.CreateFromFile(typeof(T).Assembly.Location)]);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "GeneratorTests",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult runResult = await TestHelpers.RunGeneratorAndAssertOutput<T>(compilation, stages);

        return (runResult.Diagnostics, runResult.GeneratedTrees.Select(gs => gs.ToString()).ToArray());
    }
}
