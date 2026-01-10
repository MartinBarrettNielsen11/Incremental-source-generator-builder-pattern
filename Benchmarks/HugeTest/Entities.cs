using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Benchmarks.HugeTest;

/*************************************************************/
/* The following Entities are annotated for source generation */
/*************************************************************/

public class Entity1
{
    public Guid Id { get; set; }
    public IList<Entity2> EntityList { get; set; } = new List<Entity2>();
}

public class Entity2
{
    public Entity2()
    {
        EntityList = new List<Entity3>();
    }
    public Guid Id { get; set; }
    public string Val { get; set; }
    public IList<Entity3> EntityList { get; }
}

public class Entity3
{
    private List<Entity4_0> EntityList { get; } = new List<Entity4_0>();
    public int MinimumPrice { get; set; }
    public int? MaximumPrice { get; set; }
}

public class Entity4
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Entity5
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}


public class Entity6
{
    public Guid Id { get; set; }
    public Entity6 Metadata { get; set; } = new();
}

public enum Status { Pending, Completed, Failed }

public class Entity7
{
    public int Id { get; set; }
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}


public class Entity8
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = "Default";
        
    public void Rename(string newTitle) => Title = newTitle;
}

public class Entity9
{
    public Entity9(IEnumerable<string> items)
    {
        Items = new List<string>(items);
    }

    public IList<string> Items { get; }
}

public class Entity10
{
    private readonly List<string> _tags = new();
    public ReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public void AddTag(string tag) => _tags.Add(tag);
}

public class Entity11
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
    public int GetQuantity() => Quantity;
    public void SetId(int value) => Quantity = value;
}

public abstract class BaseEntity
{
    protected BaseEntity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}


public class Entity12 : BaseEntity
{
    public Entity12(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; set; }
}

public class Entity13
{
    public (int Min, int Max) Range { get; set; }
    public record NestedRecord(string Key, string Value);
    public NestedRecord Data { get; set; } = new("A", "B");
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}

public class Entity14
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class Entity15
{
    public string? Notes { get; set; }
    public int? Rating { get; set; }
    public bool IsPublished { get; set; } = true;
    public required bool HasNotes { get; set; } = false;
}

public class Entity16
{
    private Entity16(Guid id) => Id = id;
    public Guid Id { get; }

    public static Entity16 Create() => new(Guid.NewGuid());
}

public class Entity17
{
    public Guid Id { get; set; }
    public event EventHandler? OnChanged;

    public void TriggerChange() => OnChanged?.Invoke(this, EventArgs.Empty);
}

public class Entity18
{
    private Guid _id;
    private string _name;

    public Guid GetId() => _id;
    public void SetId(Guid value) => _id = value;

    public string GetName() => _name;
    public void SetName(string value) => _name = value;
}

public class Entity19
{
    private int _age;
    public int Age
    {
        get { return _age; }
        set { _age = value < 0 ? 0 : value; }
    }
}

public class Entity20
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public int Score { get; set; }
}



/*************************************************************/
/* The following Entities are not annotated by the source generator */
/*************************************************************/
#region Entities_0

public class Entity1_0
{
    public Guid Id { get; set; }
    public IList<Entity2_0> EntityList { get; set; } = new List<Entity2_0>();
}

public class Entity2_0
{
    public Entity2_0()
    {
        EntityList = new List<Entity3_0>();
    }
    
    public Guid Id { get; set; }

    public string Val { get; set; }
    
    public IList<Entity3_0> EntityList { get; }
}

public class Entity3_0
{
    private List<Entity4_0> EntityList { get; } = new List<Entity4_0>();
    public int MinimumPrice { get; set; }
    public int? MaximumPrice { get; set; }
}

public class Entity4_0
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Entity5_0
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public class Entity6_0
{
    public Guid Id { get; set; }
    public Entity6 Metadata { get; set; } = new();
}

public enum Status_0 { Pending, Completed, Failed }

public class Entity7_0
{
    public int Id { get; set; }
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}

public class Entity8_0
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = "Default";
        
    public void Rename(string newTitle) => Title = newTitle;
}

public class Entity9_0
{
    public Entity9_0(IEnumerable<string> items)
    {
        Items = new List<string>(items);
    }

    public IList<string> Items { get; }
}

public class Entity10_0
{
    private readonly List<string> _tags = new();
    public ReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public void AddTag(string tag) => _tags.Add(tag);
}

public class Entity11_0
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
    public int GetQuantity() => Quantity;
    public void SetId(int value) => Quantity = value;
}

public abstract class BaseEntity_0
{
    protected BaseEntity_0(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}


public class Entity12_0 : BaseEntity
{
    public Entity12_0(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; set; }
}

public class Entity13_0
{
    public (int Min, int Max) Range { get; set; }
    public record NestedRecord(string Key, string Value);
    public NestedRecord Data { get; set; } = new("A", "B");
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}

public class Entity14_0
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class Entity15_0
{
    public string? Notes { get; set; }
    public int? Rating { get; set; }
    public bool IsPublished { get; set; } = true;
    public required bool HasNotes { get; set; } = false;
}

public class Entity16_0
{
    private Entity16_0(Guid id) => Id = id;
    public Guid Id { get; }

    public static Entity16_0 Create() => new(Guid.NewGuid());
}

public class Entity17_0
{
    public Guid Id { get; set; }
    public event EventHandler? OnChanged;

    public void TriggerChange() => OnChanged?.Invoke(this, EventArgs.Empty);
}

public class Entity18_0
{
    private Guid _id;
    private string _name;

    public Guid GetId() => _id;
    public void SetId(Guid value) => _id = value;

    public string GetName() => _name;
    public void SetName(string value) => _name = value;
}

public class Entity19_0
{
    private int _age;
    public int Age
    {
        get { return _age; }
        set { _age = value < 0 ? 0 : value; }
    }
}

public class Entity20_0
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public int Score { get; set; }
}

#endregion


#region Entities_1

public class Entity1_1
{
    public Guid Id { get; set; }
    public IList<Entity2_1> EntityList { get; set; } = new List<Entity2_1>();
}

public class Entity2_1
{
    public Entity2_1()
    {
        EntityList = new List<Entity3_1>();
    }
    
    public Guid Id { get; set; }

    public string Val { get; set; }
    
    public IList<Entity3_1> EntityList { get; }
}

public class Entity3_1
{
    private List<Entity4_1> EntityList { get; } = new List<Entity4_1>();
    public int MinimumPrice { get; set; }
    public int? MaximumPrice { get; set; }
}

public class Entity4_1
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Entity5_1
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public class Entity6_1
{
    public Guid Id { get; set; }
    public Entity6 Metadata { get; set; } = new();
}

public class Entity7_1
{
    public int Id { get; set; }
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}

public class Entity8_1
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = "Default";
        
    public void Rename(string newTitle) => Title = newTitle;
}

public class Entity9_1
{
    public Entity9_1(IEnumerable<string> items)
    {
        Items = new List<string>(items);
    }

    public IList<string> Items { get; }
}

public class Entity10_1
{
    private readonly List<string> _tags = new();
    public ReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public void AddTag(string tag) => _tags.Add(tag);
}

public class Entity11_1
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
    public int GetQuantity() => Quantity;
    public void SetId(int value) => Quantity = value;
}

public class Entity12_1 : BaseEntity
{
    public Entity12_1(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; set; }
}

public class Entity13_1
{
    public (int Min, int Max) Range { get; set; }
    public record NestedRecord(string Key, string Value);
    public NestedRecord Data { get; set; } = new("A", "B");
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}

public class Entity14_1
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class Entity15_1
{
    public string? Notes { get; set; }
    public int? Rating { get; set; }
    public bool IsPublished { get; set; } = true;
    public required bool HasNotes { get; set; } = false;
}

public class Entity16_1
{
    private Entity16_1(Guid id) => Id = id;
    public Guid Id { get; }

    public static Entity16_1 Create() => new(Guid.NewGuid());
}

public class Entity17_1
{
    public Guid Id { get; set; }
    public event EventHandler? OnChanged;

    public void TriggerChange() => OnChanged?.Invoke(this, EventArgs.Empty);
}

public class Entity18_1
{
    private Guid _id;
    private string _name;

    public Guid GetId() => _id;
    public void SetId(Guid value) => _id = value;

    public string GetName() => _name;
    public void SetName(string value) => _name = value;
}

public class Entity19_1
{
    private int _age;
    public int Age
    {
        get { return _age; }
        set { _age = value < 0 ? 0 : value; }
    }
}

public class Entity20_1
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public int Score { get; set; }
}

#endregion


#region Entities_2

public class Entity1_2
{
    public Guid Id { get; set; }
    public IList<Entity2_2> EntityList { get; set; } = new List<Entity2_2>();
}

public class Entity2_2
{
    public Entity2_2()
    {
        EntityList = new List<Entity3_2>();
    }
    
    public Guid Id { get; set; }

    public string Val { get; set; }
    
    public IList<Entity3_2> EntityList { get; }
}

public class Entity3_2
{
    private List<Entity4_2> EntityList { get; } = new List<Entity4_2>();
    public int MinimumPrice { get; set; }
    public int? MaximumPrice { get; set; }
}

public class Entity4_2
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Entity5_2
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public class Entity6_2
{
    public Guid Id { get; set; }
    public Entity6 Metadata { get; set; } = new();
}

public class Entity7_2
{
    public int Id { get; set; }
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}

public class Entity8_2
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = "Default";
        
    public void Rename(string newTitle) => Title = newTitle;
}

public class Entity9_2
{
    public Entity9_2(IEnumerable<string> items)
    {
        Items = new List<string>(items);
    }

    public IList<string> Items { get; }
}

public class Entity10_2
{
    private readonly List<string> _tags = new();
    public ReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public void AddTag(string tag) => _tags.Add(tag);
}

public class Entity11_2
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total => Quantity * Price;
    public int GetQuantity() => Quantity;
    public void SetId(int value) => Quantity = value;
}

public class Entity12_2 : BaseEntity
{
    public Entity12_2(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; set; }
}

public class Entity13_2
{
    public (int Min, int Max) Range { get; set; }
    public record NestedRecord(string Key, string Value);
    public NestedRecord Data { get; set; } = new("A", "B");
    public Status CurrentStatus { get; set; }
    public Status? PreviousStatus { get; set; }
}

public class Entity14_2
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class Entity15_2
{
    public string? Notes { get; set; }
    public int? Rating { get; set; }
    public bool IsPublished { get; set; } = true;
    public required bool HasNotes { get; set; } = false;
}

public class Entity16_2
{
    private Entity16_2(Guid id) => Id = id;
    public Guid Id { get; }

    public static Entity16_2 Create() => new(Guid.NewGuid());
}

public class Entity17_2
{
    public Guid Id { get; set; }
    public event EventHandler? OnChanged;

    public void TriggerChange() => OnChanged?.Invoke(this, EventArgs.Empty);
}

public class Entity18_2
{
    private Guid _id;
    private string _name;

    public Guid GetId() => _id;
    public void SetId(Guid value) => _id = value;

    public string GetName() => _name;
    public void SetName(string value) => _name = value;
}

public class Entity19_2
{
    private int _age;
    public int Age
    {
        get { return _age; }
        set { _age = value < 0 ? 0 : value; }
    }
}

public class Entity20_2
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public int Score { get; set; }
}

#endregion