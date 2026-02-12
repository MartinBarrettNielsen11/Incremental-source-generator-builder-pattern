namespace Generator;

internal static class BuilderSourceWriter
{
    internal static string GenerateBuilderAttribute(string builderAttributeName) =>
        $$$"""
           #nullable enable
           #pragma warning disable CA1813, CA1019, IDE0065, IDE0034, IDE0055

           using System;

           namespace {{{Constants.GeneratorName}}};

           [System.CodeDom.Compiler.GeneratedCode("{{{Constants.GeneratorName}}}", "{{{Constants.GeneratorVersion}}}")]
           [AttributeUsage(AttributeTargets.Class)]
           public sealed class {{{builderAttributeName}}}(Type type) : Attribute
           {
               public Type TargetType { get; } = type;
           }

           #pragma warning restore CA1813, CA1019, IDE0065, IDE0034, IDE0055
           """;

    internal static string GenerateDomainAssertionExtensions(string builderProduct) =>
        $$$"""
           #pragma warning disable IDE0055, IDE0008
           #nullable enable

           using System;
           using System.Collections.Generic;

           namespace {{{builderProduct}}};

           [System.CodeDom.Compiler.GeneratedCode("{{{Constants.GeneratorName}}}", "{{{Constants.GeneratorVersion}}}")]
           public static class {{{Constants.DomainAssertionExtensions}}}
           {
               /// <summary>
               /// Adds a domain validation rule to be executed during the build phase.
               /// </summary>
               /// <param name="{{{Constants.DomainListName}}}">The list of post-build domain rules.</param>
               /// <param name="predicate">A function that returns true when the entity violates a rule.</param>
               /// <param name="errorMessage">The error message that will be thrown if the rule fails.</param>
               public static void AddDomainRule<TEntity>(
                   this List<Action<TEntity>> {{{Constants.DomainListName}}},
                   Func<TEntity, bool> predicate,
                   string errorMessage)
               {
                   {{{Constants.DomainListName}}}.Add(e =>
                   {
                       if (predicate(e))
                           throw new InvalidOperationException(errorMessage);
                   });
               }
           }

           #pragma warning restore IDE0055, IDE0008
           """;

    internal static string GenerateBuilder(in BuilderToGenerate builder)
    {
        var estimated = EstimateInitialCapacity(builder);

        const int conservativeStackLimit = 2048;

        Span<char> initial = estimated < conservativeStackLimit
            ? stackalloc char[estimated]
            : stackalloc char[conservativeStackLimit];

        ValueStringBuilder vsb = new ValueStringBuilder(initial);

        GenerateHeader(ref vsb, builder);
        GenerateFields(ref vsb, builder);
        GenerateWithMethods(ref vsb, builder);
        GenerateBuildMethod(ref vsb, builder);
        GenerateFooter(ref vsb);

        return vsb.ToString();

    }
    

    private static void GenerateHeader(ref ValueStringBuilder vsb, in BuilderToGenerate builder)
    {
        vsb.Append(
            $$$"""
               #nullable enable
               #pragma warning disable IDE0055, IDE0008

               using System;
               using System.Linq;
               using System.Collections.Generic;
               using Generator;

               namespace {{{builder.BuilderClassNamespace}}};

               [System.CodeDom.Compiler.GeneratedCode("{{{Constants.GeneratorName}}}", "{{{Constants.GeneratorVersion}}}")]
               public partial class {{{builder.BuilderClassName}}}
               {
               
               """.AsSpan());
    }

    private static void GenerateFields(ref ValueStringBuilder vsb, in BuilderToGenerate builder)
    {
        vsb.Append($""" 
                         private Func<{builder.TargetClassFullName}> {Constants.FactoryName} = () => new();   
                         private readonly List<Action<{builder.TargetClassFullName}>> {Constants.DomainListName} = new();
                     """.AsSpan());
        vsb.Append("\n".AsSpan());
        
        foreach (PropertyInfoModel prop in builder.Properties.AllProperties)
        {
            var type = prop.TypeName;
            var camelCase = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
            vsb.Append($"    private Func<{type}>? _{camelCase};\n".AsSpan());
        }
    }

    private static void GenerateWithMethods(ref ValueStringBuilder vsb, in BuilderToGenerate builder)
    {
        foreach (PropertyInfoModel prop in builder.Properties.AllProperties)
        {
            var type = prop.TypeName;
            var name = prop.Name;
            var camelCase = char.ToLowerInvariant(name[0]) + name.Substring(1);

            vsb.Append(
                $$$"""
                   
                       public {{{builder.BuilderClassName}}} {{{Constants.WithMethodPrefix}}}{{{name}}}({{{type}}} @{{{camelCase}}})
                       {
                           return {{{Constants.WithMethodPrefix}}}{{{name}}}(() => @{{{camelCase}}});
                       }
                       public {{{builder.BuilderClassName}}} {{{Constants.WithMethodPrefix}}}{{{name}}}(Func<{{{type}}}> @{{{camelCase}}})
                       {
                           _{{{camelCase}}} = @{{{camelCase}}};
                           return this;
                       }

                   """.AsSpan());
        }
    }

    private static void GenerateBuildMethod(ref ValueStringBuilder vsb, in BuilderToGenerate builder)
    {
        vsb.Append(
            $$"""
                   /// <summary> 
                   /// Returns configured instance of {{builder.TargetClassFullName}} 
                   /// </summary>
                   public {{builder.TargetClassFullName}} Build()
                   {
                       {{builder.TargetClassFullName}} instance = {{Constants.FactoryName}}();

               """.AsSpan());

        foreach (PropertyInfoModel prop in builder.Properties.Normal)
        {
            var name = prop.Name;
            var camelCase = char.ToLowerInvariant(name[0]) + name.Substring(1);

            vsb.Append(
                $$$"""
                   
                           if(_{{{camelCase}}} is not null)
                               instance.{{{name}}} = _{{{camelCase}}}.Invoke();

                   """.AsSpan());
        }

        foreach (PropertyInfoModel prop in builder.Properties.Collection)
        {
            var name = prop.Name;
            var camelCase = char.ToLowerInvariant(name[0]) + name.Substring(1);
            vsb.Append("\n".AsSpan());

            vsb.Append(
                $"""
                           _{camelCase}?.Invoke()?
                               .ToList()
                               .ForEach(item => instance.{name}.Add(item));

                   """.AsSpan());
        }

        vsb.Append(
            $$"""
               
                       {{Constants.DomainListName}}.ForEach(action => action(instance));

                       return instance;
                   }

               """.AsSpan());
    }

    private static void GenerateFooter(ref ValueStringBuilder vsb) =>
        vsb.Append(
            $$"""
               }
               
               #pragma warning restore IDE0055, IDE0008
               """.AsSpan());
    
    private static int EstimateInitialCapacity(in BuilderToGenerate builder)
    {
        const int header = 300;
        const int footer = 100;
        const double padding = 1.05;

        List<PropertyInfoModel> allProps = builder.Properties.AllProperties;
        var collectionCount = builder.Properties.Collection.Count;

        // Compute averages
        var totalNameLen = 0;
        var totalTypeLen = 0;

        foreach (PropertyInfoModel p in allProps)
        {
            totalNameLen += p.Name.Length;
            totalTypeLen += p.TypeName.Length;
        }

        var avgNameLen = allProps.Count > 0 ? totalNameLen / allProps.Count : 0;
        var avgTypeLen = allProps.Count > 0 ? totalTypeLen / allProps.Count : 0;

        var perPropertyCost = 310 + avgNameLen + avgTypeLen;

        var total = header + allProps.Count * perPropertyCost + collectionCount * 150 + footer;

        return (int)(total * padding);
    }
}
