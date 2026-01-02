using sample;

var builderForEntity1_simple = Entity1Builder.Simple().Build();
var b = Entity1Builder.Typical().WithEntity2List(new List<Entity2>
{
    Entity2Builder.Typical().Build(), Entity2Builder.Typical().WithVal("yoooo").Build()
}).Build();

var builder = new Entity2Builder().WithId(() => Guid.NewGuid());

/* unique ids */
Console.WriteLine(builder.Build().Id);
Console.WriteLine(builder.Build().Id);

var builder2 = new Entity2Builder().WithId(Guid.NewGuid());

/* same id */
Console.WriteLine(builder2.Build().Id);
Console.WriteLine(builder2.Build().Id);

var v3 = Entity3Builder.Typical().Build();

var builderForEntity2_typical = Entity2Builder.Typical().Build();

var builderForEntity3_simple = Entity3Builder.Simple().Build();
var builderForEntity3_typical_nobuild = Entity3Builder.Typical().WithMinimumPrice(1_000);

var yo = builderForEntity3_typical_nobuild.Build();

var builderForEntity2_typical2 = Entity2Builder.Typical().Build();