using System;
using System.Collections.Generic;
using BimServices.BuilderGenerator;

namespace BimServices.BuilderGenerator.Tests.TestData;


public abstract class Base
{
    public string Property1 { get; set; }
}

public class Entity2
{
    public string Property2 { get; set; }
}

public class Entity : Base
{
    public long Id { get; set; }
    public string Name { get; set; }
    public bool ModifiedByPostBuildAction { get; set; }
    public IList<Entity2> EntityList { get; set; } = new List<Entity2>();
}


[Builder(typeof(Entity))]
public partial class EntityBuilder
{
    public EntityBuilder()
    {
        _domainRules.Add(e => { e.ModifiedByPostBuildAction = true; });
    }
}