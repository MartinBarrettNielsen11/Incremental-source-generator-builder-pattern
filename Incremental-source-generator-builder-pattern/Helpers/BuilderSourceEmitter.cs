namespace Incremental_source_generator_builder_pattern;

internal static class BuilderSourceEmitter
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
    
    
    internal static string GenerateDomainAssertionExtensions(string builderProduct)
    {
        var vsb = new StringBuilder(1024);
        vsb.Append("#pragma warning disable IDE0055, IDE0008\n");
        vsb.Append("#nullable enable\n\n");
        vsb.Append("using System;\n");
        vsb.Append("using System.Collections.Generic;\n\n");
        vsb.Append($"namespace {builderProduct};\n\n");
        vsb.Append($"[System.CodeDom.Compiler.GeneratedCode(\"{Constants.GeneratorName}\", \"{Constants.GeneratorVersion}\")]\n");
        vsb.Append($"public static class {Constants.DomainAssertionExtensions}\n");
        vsb.Append("{\n");
        vsb.Append("    /// <summary>\n");
        vsb.Append("    /// Adds a domain validation rule to be executed during the build phase.\n");
        vsb.Append("    /// </summary>\n");
        vsb.Append("    /// <typeparam name=\"TEntity\">The entity type the rule applies to.</typeparam>\n");
        vsb.Append($"    /// <param name=\"{Constants.DomainListName}\">The list of post-build domain rules.</param>\n");
        vsb.Append("    /// <param name=\"predicate\">A function that returns true when the entity violates a rule.</param>\n");
        vsb.Append("    /// <param name=\"errorMessage\">The error message that will be thrown if the rule fails.</param>\n");
        vsb.Append("    public static void AddDomainRule<TEntity>(\n");
        vsb.Append($"        this List<Action<TEntity>> {Constants.DomainListName},\n");
        vsb.Append("        Func<TEntity, bool> predicate,\n");
        vsb.Append("        string errorMessage)\n");
        vsb.Append("    {\n");
        vsb.Append($"        {Constants.DomainListName}.Add(e =>\n");
        vsb.Append("        {\n");
        vsb.Append("            if (predicate(e))\n");
        vsb.Append("                throw new InvalidOperationException(errorMessage);\n");
        vsb.Append("        });\n");
        vsb.Append("    }\n");
        vsb.Append("}\n");
        vsb.Append("#pragma warning restore IDE0055, IDE0008\n");

        return vsb.ToString();
    }
    
    internal static string GenerateBuilder(BuilderToGenerate builder)
    {
        return string.Empty;
    }
    
}