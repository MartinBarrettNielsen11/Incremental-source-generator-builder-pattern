using Generator;

namespace GeneratorTests;

public class EquatableArrayTests
{
    [Test]
    public async Task Equals_When_BothHaveTheSameContentsForPrimitives()
    {
        EquatableArray<int> left = new([1, 2, 3, 4, 5]);
        EquatableArray<int> right = new([1, 2, 3, 4, 5]);

        var areEqual = left == right;

        await Assert.That(areEqual).IsTrue();
    }
    
    [Test]
    public async Task EqualHashCodes_When_BothHaveTheSameContents()
    {
        EquatableArray<int> left = new([1, 2]);
        EquatableArray<int> right = new([1, 2]);
        
        var hashcodeLeft = left.GetHashCode();
        var hashcodeRight = right.GetHashCode();
        
        await Assert.That(hashcodeLeft == hashcodeRight).IsTrue();
    }

    [Test]
    public async Task Equals_When_BothHaveTheSameContentsForRecords()
    {
        EquatableArray<TestRecord> first = new([new TestRecord(10), new TestRecord(20)]);
        EquatableArray<TestRecord> second = new([new TestRecord(10), new TestRecord(20)]);

        var result = first.Equals(second);

        await Assert.That(result).IsTrue();
    }

    // Simulates entity with value-based equality
    private record TestRecord(int Test);
}
