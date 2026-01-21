namespace Generator.Tests;

internal static class TestSourceFactoryConstants
{
    internal const string VerifyDirectory = "Snapshots";
    internal static readonly string Example1 = $"{typeof(TestSourceFactoryConstants).Namespace}.TestData.Test1.cs";
    internal static readonly string Example2 = $"{typeof(TestSourceFactoryConstants).Namespace}.TestData.Test2.cs";
    internal static readonly string Example3 = $"{typeof(TestSourceFactoryConstants).Namespace}.TestData.Test3.cs";
}