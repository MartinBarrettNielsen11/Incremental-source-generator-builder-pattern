namespace Generator;

internal static class Helpers
{
    internal static Properties GetPropertySymbols(
        INamedTypeSymbol typeICollection,
        ITypeSymbol namedTypeSymbol)
    {
        List<IPropertySymbol> normalSymbols = [];
        List<IPropertySymbol> collectionSymbols = [];

        CollectSymbols(namedTypeSymbol, typeICollection, collectionSymbols, normalSymbols);

        PropertyInfoModel[] normal = normalSymbols
            .OrderBy(p => p.Name)
            .Select(p => ToModel(p, isCollection: false))
            .ToArray();

        PropertyInfoModel[] collection = collectionSymbols
            .OrderBy(p => p.Name)
            .Select(p => ToModel(p, isCollection: true))
            .ToArray();
        
        return new Properties(
            new EquatableArray<PropertyInfoModel>(normal),
            new EquatableArray<PropertyInfoModel>(collection));
    }
    

    private static void CollectSymbols(ITypeSymbol type, 
                                       INamedTypeSymbol typeICollection, 
                                       List<IPropertySymbol>? collection, 
                                       List<IPropertySymbol>? normal)
    {
        foreach (IPropertySymbol? property in type.GetMembers().OfType<IPropertySymbol>())
        {
            var isCollection = property.SetMethod is null &&
                                ImplementsInterface(property.Type, typeICollection);
            
            if (isCollection && collection is not null)
            {
                collection.Add(property);
            }
            else if (property.SetMethod is not null && 
                     property.SetMethod.DeclaredAccessibility == Accessibility.Public &&
                     normal is not null)
            {
                normal.Add(property);
            }
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
