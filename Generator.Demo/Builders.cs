namespace Generator.Demo;

[Builder(typeof(Entity1))]
public sealed partial class Entity1Builder : IEntityBuilder<Entity1Builder, Entity1>
{
    public static Entity1Builder Minimal() => new Entity1Builder()
        .WithId(Guid.NewGuid());

    public static Entity1Builder Typical() => Minimal()
        .WithEntity2List(new List<Entity2>
        {
            Entity2Builder.Typical().Build()
        });

    public static Entity1Builder Committed() => Typical()
        .WithCount(1)
        .WithEntity3List(new List<Entity3>
        {
            Entity3Builder.Typical().Build()
        });
}

[Builder(typeof(Entity2))]
public sealed partial class Entity2Builder : IEntityBuilder<Entity2Builder, Entity2>
{
    public static Entity2Builder Minimal() => new Entity2Builder();

    public static Entity2Builder Typical() => Minimal()
        .WithEntity3List(new List<Entity3>()
        {
            Entity3Builder.Minimal().Build()
        });
}


[Builder(typeof(Entity3))]
public sealed partial class Entity3Builder : IEntityBuilder<Entity3Builder, Entity3>
{
    private Entity3Builder()
    {
        _domainRules.AddDomainRule(
            predicate: e => e.MaximumPrice < e.MinimumPrice, 
            errorMessage: "MaximumPrice must be larger than MinimumPrice");
    }
    
    public static Entity3Builder Minimal() =>  new Entity3Builder().WithMinimumPrice(10);

    public static Entity3Builder Typical() => Minimal().WithMaximumPrice(20);
}