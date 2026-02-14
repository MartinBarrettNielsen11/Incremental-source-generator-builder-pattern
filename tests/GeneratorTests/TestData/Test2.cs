using Generator;

namespace GeneratorTests.TestData.Legacy
{
    public class TestEntity
    {
        public int TestEntityId { get; set; }
    }

    [Builder(typeof(TestEntity))]
    public partial class TestEntityBuilder { }
}

namespace GeneratorTests.TestData
{
    public class TestEntity
    {
        public int TestEntityId { get; set; }
        public string Name { get; set; }
    }

    [Builder(typeof(TestEntity))]
    public partial class TestEntityBuilder { }
}