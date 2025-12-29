# BimServices.BuilderGenerator

*** DRAFT VERSION OF THIS DOCUMENT ***

## Introduction
The BimServices.BuilderGenerator is a Roslyn powered incremental code generator.  Its sole purpose is to semi-automatically generate Builder classes for use in tests.
E.g. if we have an entity named Citizen with properties (Cpr, FirstName, LastName), the generator can generate code so that the following is possible:

```csharp
var citizen = CitizenBuilder
                    .WithCpr("2512484916")
                    .WithFirstName("Nancy")
                    .WithLastName("Berggren")
                .Build();
```


## How to use in project
Add a folder/namespace in which you plan to have your builders located (e.g. named *Builders*)
In the *Builders* folder, add a builder for a given entity. This is just a public partial class decorated with a *BuilderFor* attribute that describes for which entity the builder should be created.

```csharp
namespace BimServices.BlahProject.Test.Builders
{
    using BimServices.BuilderGenerator;           // So we can use the BuilderFor attribute
    using Domain.Citizen;                   // For where the domain entity is placed
   
    [BuilderFor(typeof(Citizen))]
    public partial class CitizenBuilder
    {
    }
}
```


Whenever you compile your project the BuilderGenerator automatically analyzes the project for all classes decorated with the *BuilderFor* attribute.  Each decorated class, refers to an entity type that again will be analyzed - now for all the *public* properties with set methods.  The analyzing is performed recursively through the inheritance hierachy.

*With*-Methods for all found properties are generated on the *Builder* class in an in-memory part of the partial class. This means you will never see the actual generated code, but rather see it as part of the code-completion when referring the connected entity.
