# BuilderGenerator

## Introduction
The BimServices.BuilderGenerator is a Roslyn powered incremental source generator.
Its purpose is to automate the generation of object builders for testing - using a fluent syntax
It generates the repetitive part of creating builders, leaving only the more interesting, hand-curated parts for you to implement as partial classes.

An example usage for a domain entity can be seen in the following:

```csharp
var site = buildingBuilder
                .WithName("building1")
                .WithCreatedAt("2025-12-24")
                .WithRevision(2)
            .Build();
```

## Installation
Add the following project reference in consuming project:
```xml
        <ProjectReference Include="..\BimServices.BuilderGenerator\BimServices.BuilderGenerator.csproj"
                          OutputItemType = "Analyzer"
                          ReferenceOutputAssembly="false"/>
```


## How to use in consuming project
Add a folder/namespace in which you plan to have your builders located (e.g. named *Builders*).
In the *Builders* folder, add a builder for the given entity.
This is just a public partial class decorated with a *Builder* attribute that describes for which entity the builder should be created:

```csharp
namespace BimServices.Tests.Data.Builders;

using BimServices.BuilderGenerator;
using Domain.Site;

[BuilderFor(typeof(Site))]
public partial class SiteBuilder { }
```
And now you have all the code necessary for using your builders.

## Under the hood
The generated source code comprises the repetitive, boring part of the process, including the backing field and ”With” methods that provide the fluent builder syntax.

Thus, for a domain object like the following:

```csharp
public class Site
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Revision { get; set; }
    public IList<SiteRights> SiteRights { get; set; } = new List<SiteRights>();
}
```

## Advised usage
The proceeding subsections will paint a picture of how the builders can be set up such that they serve as living documentation for the entity, communicating common usage patterns, invariants, and intent directly to developers working with the system.

### Static factory methods

A common and recommended pattern is to define a small hierarchy of static factory methods on each builder. These methods represent specific domain scenarios and serve as stable, intentional starting points for tests.

For example, a `SiteBuilder` might define the following:

- **Minimal**
    - Produces the smallest possible instance that satisfies all invariants and can be persisted.
    - Serves as the foundation for all other factory methods.
    - When a new required field is added to the domain model, updating this method is often sufficient to fix most tests.

- **Typical**
    - Represents a realistic, commonly used configuration of the entity.
    - Builds on `Minimal` and fills in additional relationships or values.
    - Intended to reflect how the entity is most often encountered in real scenarios.

- **Scenario-specific factories**
    - Additional factory methods (e.g. `AdminSite`) may be introduced when a particular configuration appears repeatedly across tests.
    - These methods encode recurring test scenarios directly into the builder, reducing duplication and improving clarity.
