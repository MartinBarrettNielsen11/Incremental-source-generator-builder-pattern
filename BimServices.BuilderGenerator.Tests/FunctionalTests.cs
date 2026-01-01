using Microsoft.CodeAnalysis;

namespace BimServices.BuilderGenerator.Tests;

internal class FunctionalTests
{
    private static readonly string Example1 = $"{typeof(FunctionalTests).Namespace}.TestData.Test1.cs";

    private VerifySettings _settings = new();

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
}