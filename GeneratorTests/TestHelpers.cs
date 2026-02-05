using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Assembly = System.Reflection.Assembly;

namespace GeneratorTests;

internal static class TestHelpers
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
            assemblyName: "GeneratorTests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new Generator.Generator();

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
    
    
    internal static async Task<(ImmutableArray<Diagnostic> diagnostics, string[] Output)> GetGeneratedTrees<T>(
        string[] sources, 
        string[] stages) 
        where T : IIncrementalGenerator, new()
    {
        IEnumerable<SyntaxTree> syntaxTrees = sources.Select(static s => CSharpSyntaxTree.ParseText(s));

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
            .Select(_ => MetadataReference.CreateFromFile(_.Location))
            .Concat(new[] { MetadataReference.CreateFromFile(typeof(T).Assembly.Location) });

        CSharpCompilation compilation = CSharpCompilation.Create(
            "BimServices.BuilderGenerator.Build",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult runResult = await RunGeneratorAndAssertOutput<T>(compilation, stages);

        return (runResult.Diagnostics, runResult.GeneratedTrees.Select(gs => gs.ToString()).ToArray());
    }
    
    
    internal static async Task<GeneratorDriverRunResult> RunGeneratorAndAssertOutput<T>(
        CSharpCompilation compilation, 
        string[] trackingNames) 
        where T : IIncrementalGenerator, new()
    {
        ISourceGenerator generator = new T().AsSourceGenerator();

        // Tell the driver to track all the incremental generator outputs
        var opts = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true);

        GeneratorDriver driver = CSharpGeneratorDriver.Create([generator], driverOptions: opts);

        // Create a clone of the compilation that we will use later
        var clone = compilation.Clone();

        // Do the initial run
        driver = driver.RunGenerators(compilation);
        GeneratorDriverRunResult runResult = driver.GetRunResult();
        GeneratorDriverRunResult runResult2 = driver.RunGenerators(clone).GetRunResult();

        await AssertRunsEqual(runResult, runResult2, trackingNames);

        // Verify the second run only generated cached source outputs
        IEnumerable<(object Value, IncrementalStepRunReason Reason)> trackedOutputs =
            runResult2.Results[0].TrackedOutputSteps
                .SelectMany(x => x.Value)
                .SelectMany(x => x.Outputs);

        await Assert.That(trackedOutputs.Select(ss => ss.Reason))
            .ContainsOnly(x => x == IncrementalStepRunReason.Cached);

        return runResult;
    }
    
    
    private static async Task AssertRunsEqual(
        GeneratorDriverRunResult runResult1,
        GeneratorDriverRunResult runResult2,
        string[] trackingNames)
    {
        var trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        var trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        // both runs should have the same tracked steps
        await Assert.That(trackedSteps1.Count).IsNotEqualTo(0).And.IsEqualTo(trackedSteps2.Count);

        foreach (var (trackingName, runSteps1) in trackedSteps1)
        {
            var runSteps2 = trackedSteps2[trackingName];
            await AssertRunsEqual(runSteps1, runSteps2, trackingName);
        }

        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(
            GeneratorDriverRunResult runResult, string[] trackingNames)
        {
            return runResult.Results[0] // We are only running a single generator, so this is safe
                .TrackedSteps
                .Where(step => trackingNames.Contains(step.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
    
    
    internal static string[] GetTrackingNames(Type trackingNamesType) => trackingNamesType
        .GetFields()
        .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
        .Select(x => (string?)x.GetRawConstantValue()!)
        .Where(x => !string.IsNullOrEmpty(x))
        .ToArray();
    
    private static async Task AssertRunsEqual(
        ImmutableArray<IncrementalGeneratorRunStep> runSteps1,
        ImmutableArray<IncrementalGeneratorRunStep> runSteps2,
        string stepName)
    {
        await Assert.That(runSteps1.Length).IsEqualTo(runSteps2.Length);

        for (var i = 0; i < runSteps1.Length; i++)
        {
            var runStep1 = runSteps1[i];
            var runStep2 = runSteps2[i];

            // The outputs should be equal between different runs
            IEnumerable<object> outputs1 = runStep1.Outputs.Select(x => x.Value);
            IEnumerable<object> outputs2 = runStep2.Outputs.Select(x => x.Value);

            await Assert.That(outputs1).IsEqualTo(outputs2);

            // The second run’s results should always be Cached or Unchanged
            await Assert.That((IEnumerable<(object Value, IncrementalStepRunReason Reason)>)runStep2.Outputs)
                .All(
                    x => x.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
                    "Each output reason should be either Cached or Unchanged"
                );

            // make sure we are not using anything we shouldn't 
            await AssertObjectGraph(runStep1, stepName);
        }
    }
    
    
    private static async Task AssertObjectGraph(IncrementalGeneratorRunStep runStep, string stepName)
    {
        // Including the stepName in error messages to make it easy to isolate issues
        var because = $"{stepName} shouldn't contain banned symbols";
        var visited = new HashSet<object>();

        foreach (var (obj, _) in runStep.Outputs)
        {
            await Visit(obj);
        }

        async Task Visit(object node)
        {
            if (node is null || !visited.Add(node))
                return;

            await Assert.That(node).IsNotAssignableTo<Compilation>()
                .And.IsNotAssignableTo<ISymbol>()
                .And.IsNotAssignableTo<SyntaxNode>();

            // Examine the object
            Type type = node.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
                return;

            // If the object is a collection, check each of the values
            if (node is IEnumerable collection and not string)
            {
                foreach (object element in collection)
                {
                    await Visit(element);
                }

                return;
            }

            //Recursively check each field in the object 
            foreach (FieldInfo f in
                     type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object fieldValue = f.GetValue(node);
                await Visit(fieldValue);
            }
        }
    }
}