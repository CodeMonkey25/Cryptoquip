using Cryptoquip.Extensions;

namespace Cryptoquip.Tests.Extensions;

public class ReadOnlyMemoryExtensionsTests
{
    [Fact]
    public void Split_Char_SplitsCorrectly()
    {
        ReadOnlyMemory<char> source = "hello world test".AsMemory();
        var parts = source.Split(' ').Select(m => m.ToString()).ToArray();

        Assert.Equal(["hello", "world", "test"], parts);
    }

    [Fact]
    public void Split_Char_WithOptions_RemoveEmptyEntries()
    {
        ReadOnlyMemory<char> source = "hello  world   test".AsMemory();
        var parts = source.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(m => m.ToString()).ToArray();

        Assert.Equal(["hello", "world", "test"], parts);
    }

    [Fact]
    public void Split_SeparatorSpan_SplitsCorrectly()
    {
        ReadOnlyMemory<char> source = "apple::banana::cherry".AsMemory();
        var parts = source.Split("::".AsSpan()).Select(m => m.ToString()).ToArray();

        Assert.Equal(["apple", "banana", "cherry"], parts);
    }

    [Fact]
    public void Split_LargeInput_ExceedingStackAllocThreshold_RentsCorrectly()
    {
        string[] words = Enumerable.Range(0, 200).Select(i => $"word{i}").ToArray();
        string input = string.Join(",", words);
        ReadOnlyMemory<char> source = input.AsMemory();

        var parts = source.Split(',').Select(m => m.ToString()).ToArray();

        Assert.Equal(words, parts);
    }
}
