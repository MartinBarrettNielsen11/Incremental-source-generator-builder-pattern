namespace Generator.Tests;

public class EquatableArrayTests
{
    [Test]
    public async Task Equals_When_BothHaveTheSameContentsForPrimitives()
    {
        var left = new EquatableArray<int>([1, 2, 3, 4, 5]);
        var right = new EquatableArray<int>([1, 2, 3, 4, 5]);

        var areEqual = left == right;

        await Assert.That(areEqual).IsTrue();
    }
    
    [Test]
    public async Task EqualHashCodes_When_BothHaveTheSameContents()
    {
        var left = new EquatableArray<int>([1, 2]);
        var right = new EquatableArray<int>([1, 2]);
        
        var hashcodeLeft = left.GetHashCode();
        var hashcodeRight = right.GetHashCode();
        
        await Assert.That(hashcodeLeft == hashcodeRight).IsTrue();
    }

    [Test]
    public async Task Equals_When_BothHaveTheSameContentsForRecords()
    {
        var first = new EquatableArray<TestRecord>([new TestRecord(10), new TestRecord(20)]);
        var second = new EquatableArray<TestRecord>([new TestRecord(10), new TestRecord(20)]);

        var result = first.Equals(second);

        await Assert.That(result).IsTrue();
    }

    // Simulates entity with value-based equality
    private record TestRecord(int Test);
}