using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Generator.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class GeneratorBenchmarks
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
     *| Method                                   | Mean     | Error     | StdDev    | Median   | Gen0    | Gen1   | Allocated |
       |----------------------------------------- |---------:|----------:|----------:|---------:|--------:|-------:|----------:|
       | Then_the_expected_output_is_generated    | 3.151 ms | 0.0934 ms | 0.2740 ms | 3.063 ms |  7.8125 |      - |  62.47 KB |
       | Then_the_output_is_generated_big_version | 3.518 ms | 0.0675 ms | 0.0693 ms | 3.510 ms | 27.3438 | 7.8125 | 188.77 KB |

    // optimized struct copying
     * | Method                                   | Mean     | Error     | StdDev    | Median   | Gen0    | Gen1   | Allocated |
         |----------------------------------------- |---------:|----------:|----------:|---------:|--------:|-------:|----------:|
         | Then_the_expected_output_is_generated    | 2.886 ms | 0.0553 ms | 0.1335 ms | 2.837 ms |  7.8125 |      - |  61.23 KB |
         | Then_the_output_is_generated_big_version | 3.414 ms | 0.0670 ms | 0.0594 ms | 3.395 ms | 27.3438 | 7.8125 | 187.53 KB |
         
     */
    
    [Benchmark]
    public GeneratorDriverRunResult Then_the_expected_output_is_generated()
    {
        Generator generator = new();
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(_citizenInput);
        CSharpCompilation compilation = CSharpCompilation.Create(
            nameof(Then_the_expected_output_is_generated),
            [syntaxTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);
        GeneratorDriverRunResult result = driver.GetRunResult();

        return result;
    }
    
    [Benchmark]
    public GeneratorDriverRunResult Then_the_output_is_generated_big_version()
    {
        Generator generator = new();
        SyntaxTree syntaxTree1 = CSharpSyntaxTree.ParseText(_entitiesInput);
        SyntaxTree syntaxTree2 = CSharpSyntaxTree.ParseText(_buildersInput);
        CSharpCompilation compilation = CSharpCompilation.Create(
            nameof(Then_the_output_is_generated_big_version),
            [syntaxTree1, syntaxTree2],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation);
        GeneratorDriverRunResult result = driver.GetRunResult();

        return result;
    }
    
    private static string GetResourceAsString(string resourceName)  
    {
        Assembly assembly = typeof(GeneratorBenchmarks).Assembly;
        var manifestResourceNames = assembly.GetManifestResourceNames();  
        resourceName = manifestResourceNames.Single(x => x.Equals($"Generator.Benchmarks.{resourceName}", StringComparison.OrdinalIgnoreCase));

        using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");  
        using StreamReader reader = new(stream);  

        return reader.ReadToEnd();  
    }  
}
