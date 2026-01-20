using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TUnit.Core.Helpers;

namespace Generator.Tests;

public class FunctionalTests
{
    private static readonly string Example1 = $"{typeof(FunctionalTests).Namespace}.TestData.Test1.cs";
    private static readonly string Example2 = $"{typeof(FunctionalTests).Namespace}.TestData.Test2.cs";
    private static readonly string Example3 = $"{typeof(FunctionalTests).Namespace}.TestData.Test3.cs";
    
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
    
    [Test]
    public async Task When_GeneratingBuilder_Then_BuilderClassIsGenerated()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];
        var generatedBuilderClass = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "Generator_Tests_TestData_EntityBuilder.cs")
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

        var (diagnostics, output) = await TestHelpers.GetGeneratedTrees<global::Generator.Generator>([input], stages);

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
        var builder1TypeName = "Generator.Tests.TestData.v1.TestEntityBuilder";
        var builder2TypeName = "Generator.Tests.TestData.v2.TestEntityBuilder";
        
        Stream mrs = typeof(FunctionalTests).Assembly.GetManifestResourceStream(Example2)!;
        string source = SourceText.From(mrs).ToString();
        var (runResult, compiledAssembly) = await TestHelpers.ParseAndDriveResult(source);
        
        await Assert.That(runResult.GeneratedTrees.Length).IsEqualTo(4);
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Generator/BimServices.BuilderGenerator.BuilderGenerator/BimServices_BuilderGenerator_Tests_TestData_v1_TestEntityBuilder.cs")).IsTrue();
        
        object? builder1 = compiledAssembly.CreateInstance(builder1TypeName);
        await Assert.That(builder1).IsNotNull();
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Generator/BimServices.BuilderGenerator.BuilderGenerator/BimServices_BuilderGenerator_Tests_TestData_v2_TestEntityBuilder.cs")).IsTrue();

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
    
    [Test]
    public async Task Correct_methods_are_generated_checked_via_reflection()
    {
        var builderTypeName = "Generator.Tests.TestData.EntityBuilder";
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance(builderTypeName); 
        var methods = testBuilder!.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        _settings.UseDirectory("Snapshots");
        await Verify(methods, _settings);
    }
    
    [Test]
    public async Task Generated_methods_can_be_called_via_reflection_for_with_methods_DIRECT_VERSION()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance("BimServices.BuilderGenerator.Generator.Tests.TestData.EntityBuilder");
        MethodInfo directGeneratedWithMethod = testBuilder!
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("WithName"))
            .Single(m => m.GetParameters()[0].ParameterType == typeof(string));

        var newName = "new-name";
        
        object? res = directGeneratedWithMethod!.Invoke(testBuilder, [ newName ]);
        await Assert.That(testBuilder).IsEqualTo(res);
    }
    
    [Test]
    public async Task Generated_methods_can_be_called_via_reflection_for_with_methods2()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance("BimServices.BuilderGenerator.Generator.Tests.TestData.EntityBuilder");
        MethodInfo directGeneratedWithMethod = testBuilder!
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("WithName"))
            .Single(m => m.GetParameters()[0].ParameterType == typeof(string));

        var newName = "new-name";
        
        object? res = directGeneratedWithMethod!.Invoke(testBuilder, [ newName ]);
        await Assert.That(testBuilder).IsEqualTo(res);
        
        MethodInfo? buildMethod = testBuilder.GetType().GetMethod("Build");
        object? entity = buildMethod!.Invoke(testBuilder, null);
        
        _settings.UseDirectory("Snapshots");
        await Verify(entity.ToObjectArray(), _settings);
    }

    
    [Test]
    public async Task Generated_methods_can_be_called_via_reflection_for_with_methods_on_collections()
    {
        var sourceText = await TestHelpers.GetSourceText(Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance("Generator.Tests.TestData.EntityBuilder");

        var entity2Type = testBuilder!.GetType().Assembly
            .GetType("BimServices.BuilderGenerator.Generator.Tests.TestData.Entity2");
        
        var listType = typeof(List<>).MakeGenericType(entity2Type!);
        object countriesList = Activator.CreateInstance(listType)!;

        PropertyInfo? piCountryName = entity2Type!.GetProperty("Property2");

        object? countryDk = Activator.CreateInstance(entity2Type);
        piCountryName!.SetValue(countryDk, "DK");

        object? countryUs = Activator.CreateInstance(entity2Type);
        piCountryName.SetValue(countryUs, "US");

        listType.InvokeMember(
            "Add",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
            binder: null,
            target: countriesList,
            args: [countryDk]);

        listType.InvokeMember(
            "Add",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
            binder: null,
            target: countriesList,
            args: [countryUs]);

        MethodInfo? miWithEntityList = testBuilder.GetType()
            .GetMethod("WithEntityList", [listType]);

        object? resultBuilder = miWithEntityList!.Invoke(testBuilder, [countriesList]);

        await Assert.That(resultBuilder).IsEquivalentTo(testBuilder);
        
        MethodInfo? buildMethod = testBuilder.GetType().GetMethod("Build");
        object? entity = buildMethod!.Invoke(testBuilder, null);
        
        _settings.UseDirectory("Snapshots");
        await Verify(entity.ToObjectArray(), _settings);
    }
    
    
    [Test]
    public async Task When_GeneratingBuilder_for_partial_domain_entity_then_everything_works_beautifully()
    {
        // also show that the method in the partial domain is not there
        var sourceText = await TestHelpers.GetSourceText(Example3);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];
        var generatedBuilderClass = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "Generator_Tests_TestData_EntityBuilder.cs")
            .SourceText
            .ToString();
        _settings.UseDirectory("Snapshots");
        await Verify(generatedBuilderClass, _settings);
    }
}
