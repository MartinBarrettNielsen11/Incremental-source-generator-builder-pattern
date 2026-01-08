using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Incremental_source_generator_builder_pattern.Tests;

public class FunctionalTests
{
    private static readonly string Example1 = $"{typeof(FunctionalTests).Namespace}.TestData.Test1.cs";
    private static readonly string Example2 = $"{typeof(FunctionalTests).Namespace}.TestData.Test2.cs";
    
    private readonly VerifySettings _settings = new();

    [Test]
    public async Task BuilderAttributeUsage_IsGenerated()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];

        var generatedBuilderForAttribute = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "BuilderAttribute.g.cs").SourceText.ToString();
        
        _settings.UseDirectory("Snapshots");
        await Verify(generatedBuilderForAttribute, _settings);
    }
    
    [Test]
    public async Task When_GeneratingBuilder_Then_BuilderClassIsGenerated()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];
        var generatedBuilderClass = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "BimServices_BuilderGenerator_Tests_TestData_EntityBuilder.cs")
            .SourceText
            .ToString();
        _settings.UseDirectory("Snapshots");
        await Verify(generatedBuilderClass, _settings);
    }
    
    /*
     * it ensures your generator’s incremental caching doesn’t break between runs, while TestHelpers does the heavy verification
     */
    [Test]
    public async Task MemoisationTest()
    {
        var input = await TestHelpers.GetSourceText(Example1);

        // 2) the tracking names your generator uses (they must match your WithTrackingName(...) calls)
        var stages = new[]
        {
            TrackingNames.InitialExtraction,
            TrackingNames.RemovingNulls
        };

        var (diagnostics, output) = await TestHelpers.GetGeneratedTrees<Generator>([input], stages);
        
        // Assert — verify no diagnostics were emitted
        await Assert.That(diagnostics.Length)
            .IsEqualTo(0, "There should be no diagnostics during memoization test");

        // Assert — verify that something was actually generated
        await Assert.That(output.Length)
            .IsGreaterThan(0, "The generator should produce output");
    }

    
    [Test]
    public async Task NoDiagnosticsAreReported()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        ImmutableArray<Diagnostic> diagnostics = runResult.Diagnostics;
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }
    // add test on "Test2" to show that only one attributeFor file is generated in case you produce two separate builder classes

    [Test]
    public async Task OnlyOneBuilderClassIsGeneratedForDomainWithPartialKeyword()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        ImmutableArray<SyntaxTree> syntaxTrees = runResult.GeneratedTrees;
        await Assert.That(syntaxTrees.Length).IsEqualTo(3);
    }
    
    
    [Test]
    public async Task Multiple_builders_with_same_name_in_different_namespaces_results_in_two_builders_being_generated()
    {
        var builder1TypeName = "Incremental_source_generator_builder_pattern.Tests.TestData.v1.TestEntityBuilder";
        var builder2TypeName = "Incremental_source_generator_builder_pattern.Tests.TestData.v2.TestEntityBuilder";
        
        Stream mrs = typeof(FunctionalTests).Assembly.GetManifestResourceStream(Example2)!;
        string source = SourceText.From(mrs).ToString();
        var (runResult, compiledAssembly) = await TestHelpers.ParseAndDriveResult(source);
        
        await Assert.That(runResult.GeneratedTrees.Length).IsEqualTo(4);
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Incremental_source_generator_builder_pattern/Incremental_source_generator_builder_pattern_Tests_TestData_v1_TestEntityBuilder.cs")).IsTrue();
        
        object? builder1 = compiledAssembly.CreateInstance(builder1TypeName);
        await Assert.That(builder1).IsNotNull();
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Incremental_source_generator_builder_pattern/Incremental_source_generator_builder_pattern_Tests_TestData_v2_TestEntityBuilder.cs")).IsTrue();

        object? builder2 = compiledAssembly.CreateInstance(builder2TypeName);
        
        await Assert.That(builder2).IsNotNull();
    }

    [Test]
    public async Task Only_one_attribute_For_file_is_generated_when_generating_two_separate_builders()
    {
        var sourceText = await TestHelpers.GetSourceText(Example2);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        ImmutableArray<SyntaxTree> syntaxTrees = runResult.GeneratedTrees;
        await Assert.That(syntaxTrees.Length).IsEqualTo(4);
    }
}