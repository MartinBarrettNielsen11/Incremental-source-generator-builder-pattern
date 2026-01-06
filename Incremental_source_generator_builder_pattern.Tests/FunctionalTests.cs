using Microsoft.CodeAnalysis;

namespace Incremental_source_generator_builder_pattern.Tests;

public class FunctionalTests
{
    private static readonly string Example1 = $"{typeof(FunctionalTests).Namespace}.TestData.Test1.cs";

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

}