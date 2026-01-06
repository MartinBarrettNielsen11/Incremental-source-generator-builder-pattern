using BimServices.BuilderGenerator;

namespace Incremental_source_generator_builder_pattern.Tests.TestData.v1;
{
    public class TestEntity
    {
        public int TestEntityId { get; set; }
    }

    [Builder(typeof(TestEntity))]
    public partial class TestEntityBuilder { }
}

namespace Incremental_source_generator_builder_pattern.Tests.TestData.v2;
{
    public class TestEntity
    {
        public int TestEntityId { get; set; }
        public string Name { get; set; }
    }

    [Builder(typeof(TestEntity))]
    public partial class TestEntityBuilder { }
}