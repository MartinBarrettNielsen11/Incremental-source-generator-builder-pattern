namespace sample;

public class Entity1
{
    public Guid Id { get; set; }
    public IList<Entity2> Entity2List { get; set; } = new List<Entity2>();
}

public class Entity2
{
    public Entity2()
    {
        Entity3List = new List<Entity3>();
    }
    public Guid Id { get; set; }
    public string Val { get; set; }
    public IList<Entity3> Entity3List { get; }
}

public class Entity3
{
    public int MinimumPrice { get; set; }
    public int? MaximumPrice { get; set; }
}