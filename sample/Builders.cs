namespace sample;

[Builder(typeof(Entity1))]
public sealed partial class Entity1Builder
{
    public static Entity1Builder Simple() => new Entity1Builder()
        .WithId(Guid.NewGuid());

    public static Entity1Builder Typical() => Simple()
        .WithEntity2List(new List<Entity2>
        {
            Entity2Builder.Typical().Build()
        });
}

[Builder(typeof(Entity2))]
public sealed partial class Entity2Builder
{
    public static Entity2Builder SimpleFunc() => new Entity2Builder()
        //.WithId(Guid.NewGuid())
        .WithId(() => Guid.NewGuid())
        .WithVal("test");
    
    public static Entity2Builder SimpleDirect() => new Entity2Builder()
        //.WithId(Guid.NewGuid())
        .WithId(Guid.NewGuid)
        .WithVal("test");

    public static Entity2Builder Typical() => SimpleDirect()
        .WithEntity3List(new List<Entity3>()
        {
            Entity3Builder.Simple().Build()
        });
}


[Builder(typeof(Entity3))]
public sealed partial class Entity3Builder
{
    private Entity3Builder()
    {
        _domainRules.AddDomainRule(
            predicate: e => e.MaximumPrice < e.MinimumPrice, 
            errorMessage: "MaximumPrice must be larger than MinimumPrice");
    }
    
    public static Entity3Builder Simple() =>  new Entity3Builder().WithMinimumPrice(10);

    public static Entity3Builder Typical() => Simple().WithMaximumPrice(20);
}