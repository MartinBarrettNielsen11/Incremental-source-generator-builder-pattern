namespace Generator.Demo;

public class Entity1
{
    public Guid Id { get; set; }
    public long Count { get; set; }
    public IList<Entity2> Entity2List { get; set; } = new List<Entity2>();
    public IList<Entity3> Entity3List { get; set; } = new List<Entity3>();
    public int? MaximumPrice { get; set; }
}

public class Entity2
{
    public Guid Id { get; set; }
    public string Val { get; set; } = string.Empty;
    public IList<Entity3> Entity3List { get; set; } = new List<Entity3>();
}

public class Entity3
{
    public int MinimumPrice { get; set; }
    public int? MaximumPrice { get; set; }
}