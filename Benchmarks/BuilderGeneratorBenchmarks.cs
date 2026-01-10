using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Incremental_source_generator_builder_pattern;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class BuilderGeneratorBenchmarks
{
    private string _citizenInput = null!;
    private string _entitiesInput = null!;
    private string _buildersInput = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _citizenInput = GetResourceAsString("Simple.Citizen.cs");
        _entitiesInput = GetResourceAsString("HugeTest.Entities.cs");
        _buildersInput = GetResourceAsString("HugeTest.Builders.cs");
    }
    

    // Since applying generic ValueStringBuilder throughout
    /*
     * | Method                                   | Mean     | Error     | StdDev    | Median   | Gen0    | Gen1   | Allocated |
       |----------------------------------------- |---------:|----------:|----------:|---------:|--------:|-------:|----------:|
       | Then_the_expected_output_is_generated    | 2.886 ms | 0.0553 ms | 0.1335 ms | 2.837 ms |  7.8125 |      - |  61.23 KB |
       | Then_the_output_is_generated_big_version | 3.414 ms | 0.0670 ms | 0.0594 ms | 3.395 ms | 27.3438 | 7.8125 | 187.53 KB |
       
     */
    [Benchmark]
    public GeneratorDriverRunResult Then_the_expected_output_is_generated()
    {
        var generator = new Generator();
        var syntaxTree = CSharpSyntaxTree.ParseText(_citizenInput);
        var compilation = CSharpCompilation.Create(
            nameof(Then_the_expected_output_is_generated),
            [syntaxTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);
        var result = driver.GetRunResult();

        return result;
    }
    
    [Benchmark]
    public GeneratorDriverRunResult Then_the_output_is_generated_big_version()
    {
        var generator = new Generator();
        var syntaxTree1 = CSharpSyntaxTree.ParseText(_entitiesInput);
        var syntaxTree2 = CSharpSyntaxTree.ParseText(_buildersInput);
        var compilation = CSharpCompilation.Create(
            nameof(Then_the_output_is_generated_big_version),
            [syntaxTree1, syntaxTree2],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        
        var driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);
        var result = driver.GetRunResult();

        return result;
    }    
        
    private static string GetResourceAsString(string resourceName)  
    {
        var assembly = typeof(BuilderGeneratorBenchmarks).Assembly;
        var manifestResourceNames = assembly.GetManifestResourceNames();  
        resourceName = manifestResourceNames.Single(x => x.Equals($"BimServices.BuilderGenerator.Benchmarks.{resourceName}", StringComparison.OrdinalIgnoreCase));
  
        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");  
        using var reader = new StreamReader(stream);  
  
        return reader.ReadToEnd();  
    }  
}
