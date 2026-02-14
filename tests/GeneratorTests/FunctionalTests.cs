using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using TUnit.Core.Helpers;

namespace GeneratorTests;

public class FunctionalTests
{
    private readonly VerifySettings _settings = new();
    
    [Test]
    public async Task BuilderAttributeUsageIsGenerated(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        GeneratorRunResult generatorRunResult = runResult.Results[0];

        var generatedBuilderForAttribute = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "BuilderAttribute.g.cs").SourceText.ToString();
        
        _settings.UseDirectory(TestConstants.VerifyDirectory);
        await Verify(generatedBuilderForAttribute, _settings);
    }
    
    [Test]
    public async Task DomainAssertionExtensionsIsGenerated(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        GeneratorRunResult generatorRunResult = runResult.Results[0];

        var generatedBuilderForAttribute = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == TestConstants.ExpectedDomainExtensionHintName).SourceText
            .ToString();
        
        _settings.UseDirectory(TestConstants.VerifyDirectory);
        await Verify(generatedBuilderForAttribute, _settings);
    }
    
    [Test]
    public async Task BuilderClassIsGenerated(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        GeneratorRunResult generatorRunResult = runResult.Results[0];
        var generatedBuilderClass = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "GeneratorTests_TestData_EntityBuilder.g.cs")
            .SourceText
            .ToString();
        _settings.UseDirectory(TestConstants.VerifyDirectory);
        await Verify(generatedBuilderClass, _settings);
    }
    
    [Test]
    public async Task NoDiagnosticsAreReported(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        ImmutableArray<Diagnostic> diagnostics = runResult.Diagnostics;
        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }
    
    [Test]
    public async Task OnlyOneBuilderIsGenerated_When_EntityHasPartialKeyword(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        ImmutableArray<SyntaxTree> syntaxTrees = runResult.GeneratedTrees;
        await Assert.That(syntaxTrees.Length).IsEqualTo(3);
    }
    
    [Test]
    public async Task When_DomainEntityHasPartialKeyword_Then_BuilderClassIsGenerated(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example3);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        GeneratorRunResult generatorRunResult = runResult.Results[0];
        
        var generatedBuilderClass = generatorRunResult.GeneratedSources
            .Single(gs => gs.HintName == "GeneratorTests_TestData_EntityBuilder.g.cs")
            .SourceText
            .ToString();
        _settings.UseDirectory(TestConstants.VerifyDirectory);
        await Verify(generatedBuilderClass, _settings);
    }
    
    [Test]
    public async Task TwoBuildersAreGenerated_When_TwoBuildersWithTheSameNameExistInDifferentNamespaces(CancellationToken ct)
    {
        const string builder1TypeName = "GeneratorTests.TestData.Legacy.TestEntityBuilder";
        const string builder2TypeName = "GeneratorTests.TestData.TestEntityBuilder";
        
        Stream mrs = typeof(FunctionalTests).Assembly.GetManifestResourceStream(TestConstants.Example2)!;
        var source = SourceText.From(mrs).ToString();
        (GeneratorDriverRunResult runResult, Assembly compiledAssembly) = await TestHelpers.ParseAndDriveResult(source, ct);
        
        await Assert.That(runResult.GeneratedTrees.Length).IsEqualTo(4);
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Generator/Generator.Generator/GeneratorTests_TestData_Legacy_TestEntityBuilder.g.cs")).IsTrue();
        
        var builder1 = compiledAssembly.CreateInstance(builder1TypeName);
        await Assert.That(builder1).IsNotNull();
        
        await Assert.That(runResult.GeneratedTrees
            .Any(gt => gt.FilePath == "Generator/Generator.Generator/GeneratorTests_TestData_TestEntityBuilder.g.cs")).IsTrue();

        var builder2 = compiledAssembly.CreateInstance(builder2TypeName);
        
        await Assert.That(builder2).IsNotNull();
    }
    
    [Test]
    public async Task OneAttributeForFileIsGenerated_When_GeneratingTwoBuilders(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example2);
        var (runResult, _) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        ImmutableArray<SyntaxTree> syntaxTrees = runResult.GeneratedTrees;
        await Assert.That(syntaxTrees.Length).IsEqualTo(4);
    }
    
    [Test]
    public async Task IntendedMethodsAreGenerated(CancellationToken ct)
    {
        const string builderTypeName = "GeneratorTests.TestData.EntityBuilder";
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        (_, Assembly compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        var testBuilder = compiledAssembly.CreateInstance(builderTypeName); 
        MethodInfo[] methods = testBuilder!.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        _settings.UseDirectory(TestConstants.VerifyDirectory);
        await Verify(methods, _settings);
    }
    
    [Test]
    public async Task NewValueIsSet_When_InvokingWithMethod(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        (_, Assembly compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        var testBuilder = compiledAssembly.CreateInstance("GeneratorTests.TestData.EntityBuilder");
        MethodInfo directGeneratedWithMethod = testBuilder!
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("WithName"))
            .Single(m => m.GetParameters()[0].ParameterType == typeof(string));

        const string newName = "new-name";
        
        var res = directGeneratedWithMethod!.Invoke(testBuilder, [ newName ]);
        await Assert.That(testBuilder).IsEqualTo(res);
    }
    
    [Test]
    public async Task EntityIsConstructed_When_InvokingBuildMethod(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        (_, Assembly compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        var testBuilder = compiledAssembly.CreateInstance("GeneratorTests.TestData.EntityBuilder");

        Type? entity2Type = testBuilder!.GetType().Assembly
            .GetType("GeneratorTests.TestData.Entity2");
        
        Type listType = typeof(List<>).MakeGenericType(entity2Type!);
        var countriesList = Activator.CreateInstance(listType)!;

        PropertyInfo? piCountryName = entity2Type!.GetProperty("Property2");

        var countryDk = Activator.CreateInstance(entity2Type);
        piCountryName!.SetValue(countryDk, "DK");

        var countryUs = Activator.CreateInstance(entity2Type);
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

        var resultBuilder = miWithEntityList!.Invoke(testBuilder, [countriesList]);

        await Assert.That(resultBuilder).IsEquivalentTo(testBuilder);
        
        MethodInfo? buildMethod = testBuilder.GetType().GetMethod("Build");
        var entity = buildMethod!.Invoke(testBuilder, null);
        
        _settings.UseDirectory("Snapshots");
        await Verify(entity.ToObjectArray(), _settings);
    }
    
    [Test]
    public async Task EntityCannotBeBuilt_When_InvokingWithMethodConflictsWithDomainRule(CancellationToken ct)
    {
        var sourceText = await TestHelpers.GetSourceText(TestConstants.Example1);
        (_, Assembly compiledAssembly) = await TestHelpers.ParseAndDriveResult(sourceText, ct);
        var testBuilder = compiledAssembly.CreateInstance("GeneratorTests.TestData.EntityBuilder");
        
        MethodInfo directGeneratedWithMethod = testBuilder!
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("WithName"))
            .Single(m => m.GetParameters()[0].ParameterType == typeof(string));

        MethodInfo buildMethod = testBuilder!.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(m => m.Name.StartsWith("Build"));
        
        const string newName = "";
        
        var res = directGeneratedWithMethod!.Invoke(testBuilder, [ newName ]);
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
