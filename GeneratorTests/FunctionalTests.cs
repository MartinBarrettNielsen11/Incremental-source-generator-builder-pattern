using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TUnit.Core.Helpers;

namespace GeneratorTests;

public class FunctionalTests
{
    private VerifySettings _settings = new();
    
    [Test]
    public async Task BuilderAttributeUsageIsGenerated()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];

        var generatedBuilderForAttribute = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "BuilderAttribute.g.cs").SourceText.ToString();
        
        _settings.UseDirectory(TestSourceFactoryConstants.VerifyDirectory);
        await Verify(generatedBuilderForAttribute, _settings);
    }
    
    [Test]
    public async Task DomainAssertionExtensionsIsGenerated()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];

        var generatedBuilderForAttribute = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "DomainAssertionExtensions.g.cs").SourceText.ToString();
        
        _settings.UseDirectory(TestSourceFactoryConstants.VerifyDirectory);
        await Verify(generatedBuilderForAttribute, _settings);
    }
    
    
    [Test]
    public async Task BuilderClassIsGenerated()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];
        var generatedBuilderClass = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "GeneratorTests_TestData_EntityBuilder.g.cs")
            .SourceText
            .ToString();
        _settings.UseDirectory(TestSourceFactoryConstants.VerifyDirectory);
        await Verify(generatedBuilderClass, _settings);
    }
    
    
    [Test]
    public async Task NoDiagnosticsAreReported()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        ImmutableArray<Diagnostic> diagnostics = runResult.Diagnostics;
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }
    
        
    [Test]
    public async Task OnlyOneBuilderIsGenerated_When_EntityHasPartialKeyword()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        ImmutableArray<SyntaxTree> syntaxTrees = runResult.GeneratedTrees;
        await Assert.That(syntaxTrees.Length).IsEqualTo(3);
    }
    
    
    [Test]
    public async Task When_DomainEntityHasPartialKeyword_Then_BuilderClassIsGenerated()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example3);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        GeneratorRunResult generatorRunResult = runResult.Results[0];
        
        var generatedBuilderClass = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "GeneratorTests_TestData_EntityBuilder.g.cs")
            .SourceText
            .ToString();
        _settings.UseDirectory(TestSourceFactoryConstants.VerifyDirectory);
        await Verify(generatedBuilderClass, _settings);
    }
    
    
    [Test]
    public async Task TwoBuildersAreGenerated_When_TwoBuildersWithTheSameNameExistInDifferentNamespaces()
    {
        var builder1TypeName = "GeneratorTests.TestData.Legacy.TestEntityBuilder";
        var builder2TypeName = "GeneratorTests.TestData.TestEntityBuilder";
        
        Stream mrs = typeof(FunctionalTests).Assembly.GetManifestResourceStream(TestSourceFactoryConstants.Example2)!;
        string source = SourceText.From(mrs).ToString();
        var (runResult, compiledAssembly) = await TestHelpers.ParseAndDriveResult(source);
        
        await Assert.That(runResult.GeneratedTrees.Length).IsEqualTo(4);
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Generator/Generator.Generator/GeneratorTests_TestData_Legacy_TestEntityBuilder.g.cs")).IsTrue();
        
        object? builder1 = compiledAssembly.CreateInstance(builder1TypeName);
        await Assert.That(builder1).IsNotNull();
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Generator/Generator.Generator/GeneratorTests_TestData_TestEntityBuilder.g.cs")).IsTrue();

        object? builder2 = compiledAssembly.CreateInstance(builder2TypeName);
        
        await Assert.That(builder2).IsNotNull();
    }
    
    [Test]
    public async Task OneAttributeForFileIsGenerated_When_GeneratingTwoBuilders()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example2);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText);
        ImmutableArray<SyntaxTree> syntaxTrees = runResult.GeneratedTrees;
        await Assert.That(syntaxTrees.Length).IsEqualTo(4);
    }
    
    [Test]
    public async Task IntendedMethodsAreGenerated()
    {
        var builderTypeName = "GeneratorTests.TestData.EntityBuilder";
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance(builderTypeName); 
        var methods = testBuilder!.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        _settings.UseDirectory(TestSourceFactoryConstants.VerifyDirectory);
        await Verify(methods, _settings);
    }
    
    [Test]
    public async Task NewValueIsSet_When_InvokingWithMethod()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance("GeneratorTests.TestData.EntityBuilder");
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
    public async Task EntityIsConstructed_When_InvokingBuildMethod()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance("GeneratorTests.TestData.EntityBuilder");

        var entity2Type = testBuilder!.GetType().Assembly
            .GetType("GeneratorTests.TestData.Entity2");
        
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
    public async Task EntityCannotBeBuilt_When_InvokingWithMethodConflictsWithDomainRule()
    {
        var sourceText = await TestHelpers.GetSourceText(TestSourceFactoryConstants.Example1);
        var (_, compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText);
        object? testBuilder = compiledAssembly.CreateInstance("GeneratorTests.TestData.EntityBuilder");
        
        MethodInfo directGeneratedWithMethod = testBuilder!
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("WithName"))
            .Single(m => m.GetParameters()[0].ParameterType == typeof(string));

        var buildMethod = testBuilder!.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name.StartsWith("Build"));
        
        const string newName = "";
        
        object? res = directGeneratedWithMethod!.Invoke(testBuilder, [ newName ]);
        await Assert.That(testBuilder).IsEqualTo(res);

        try    
        {
            buildMethod.Invoke(testBuilder, []);
            Assert.Fail("Expected InvalidOperationException, but no exception was thrown.");
        }
        catch (TargetInvocationException ex)
        {
            await Assert.That(ex.InnerException).IsTypeOf<InvalidOperationException>();
            await Assert.That(ex.InnerException!.Message).IsEqualTo("Name cannot be unspecified");
        }
    }
}
