using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Assembly = System.Reflection.Assembly;

namespace BimServices.BuilderGenerator.Tests;

internal sealed class TestHelpers
{
    internal static async Task<string> GetSourceText(string resourceName)
    {
        await using Stream mrs = typeof(FunctionalTests).Assembly.GetManifestResourceStream(resourceName)!;
        return SourceText.From(mrs).ToString();
    }
    
    internal static async Task<(GeneratorDriverRunResult runResult, Assembly compiledAssembly)>
        ParseAndDriveResult(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new Incremental_source_generator_builder_pattern.Generator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics);

        // Load assembly into memory
        using var ms = new MemoryStream();
        EmitResult emitResult = outputCompilation.Emit(ms);

        await Assert.That(emitResult.Success).IsTrue();
        var somethingToBeAsserted = string.Join("\n", emitResult.Diagnostics);
        // also assert on "diagnostics" variable

        ms.Seek(0, SeekOrigin.Begin);
        Assembly compiledAssembly = Assembly.Load(ms.ToArray());
        var runResult = driver.GetRunResult();

        return (runResult, compiledAssembly);
    }
}