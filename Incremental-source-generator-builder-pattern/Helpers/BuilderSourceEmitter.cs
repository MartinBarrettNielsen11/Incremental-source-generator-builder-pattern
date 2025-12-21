using System.Text;
using Incremental_source_generator_builder_pattern.Contracts;

namespace Incremental_source_generator_builder_pattern.Helpers;

internal sealed class BuilderSourceEmitter
{
    internal static string GenerateBuilderAttribute(string builderAttributeName)
    {
        var vsb = new StringBuilder(512);
        vsb.Append("#nullable enable\n");
        vsb.Append("#pragma warning disable CA1813, CA1019, IDE0065, IDE0034, IDE0055\n\n");
        vsb.Append("using System;\n\n");
        vsb.Append($"namespace {Constants.GeneratorName};\n\n");
        vsb.Append(
            $"[System.CodeDom.Compiler.GeneratedCode(\"{Constants.GeneratorName}\", \"{Constants.GeneratorVersion}\")]\n");
        vsb.Append("[AttributeUsage(AttributeTargets.Class)]\n");
        vsb.Append($"public sealed class {builderAttributeName}(Type type) : Attribute\n{{\n");
        vsb.Append("    public Type TargetType { get; } = type;\n");
        vsb.Append("}\n");
        vsb.Append("#pragma warning restore CA1813, CA1019, IDE0065, IDE0034, IDE0055\n");

        var yo = vsb.ToString();
        return yo;

    }
    
    internal static string GenerateBuilder(BuilderToGenerate builder)
    {
        return string.Empty;
    }
    
}