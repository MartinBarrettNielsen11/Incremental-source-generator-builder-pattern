using Incremental_source_generator_builder_pattern.Contracts;
using Microsoft.CodeAnalysis;

namespace Incremental_source_generator_builder_pattern.Helpers;


internal static class Helpers
{
    internal static Properties GetPropertySymbols(
        INamedTypeSymbol typeICollection,
        ITypeSymbol namedTypeSymbol)
    {
        var normalSymbols = new List<IPropertySymbol>();
        var collectionSymbols = new List<IPropertySymbol>();

        CollectSymbols(namedTypeSymbol, typeICollection, collectionSymbols, normalSymbols);

        var normal = normalSymbols
            .OrderBy(p => p.Name)
            .Select(p => ToModel(p, isCollection: false))
            .ToArray();

        var collection = collectionSymbols
            .OrderBy(p => p.Name)
            .Select(p => ToModel(p, isCollection: true))
            .ToArray();
        
        return new Properties(
            new List<PropertyInfoModel>(normal),
            new List<PropertyInfoModel>(collection));
    }
    

    private static void CollectSymbols(ITypeSymbol type, INamedTypeSymbol typeICollection, List<IPropertySymbol>? collection, List<IPropertySymbol>? normal)
    {
        foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
        {
            bool isCollection = property.SetMethod is null &&
                                ImplementsInterface(property.Type, typeICollection);
            if (isCollection)
                collection.Add(property);
            else if (property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
                normal.Add(property);
        }

        if (type.BaseType is { } baseType)
            CollectSymbols(baseType, typeICollection, collection, normal);
    }

    private static bool ImplementsInterface(ITypeSymbol candidate, ISymbol targetType)
    {
        return candidate.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, targetType));
    }
    
        
    private static PropertyInfoModel ToModel(IPropertySymbol property, bool isCollection) =>
        new(
            Name: property.Name,
            TypeName: property.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            IsCollection: isCollection,
            HasSetter: property.SetMethod is not null,
            Accessibility: property.DeclaredAccessibility
        );
}