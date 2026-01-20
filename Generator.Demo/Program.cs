using Generator.Demo;

Entity1 entity1V1 = Entity1Builder.Minimal()
    .WithId(Guid.CreateVersion7)
    .WithEntity2List(new List<Entity2>()
    {
        Entity2Builder.Typical()
            .WithId(Guid.CreateVersion7)
            .Build(),
    })
    .WithMaximumPrice(2_000)
    .Build();

Console.WriteLine(entity1V1);


Entity1 entity1V2 = Entity1Builder.Committed()
    .WithId(Guid.CreateVersion7)
    .Build();

Console.WriteLine(entity1V2);

var entityV2NoBuild = Entity2Builder.Typical()
    .WithEntity3List(new List<Entity3>()
    {
        Entity3Builder.Minimal().WithMaximumPrice(5).Build()
    }).Build();

Console.WriteLine(entityV2NoBuild);