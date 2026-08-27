namespace Cryptoquip.Utility;

public sealed class ReadOnlyMemoryEqualityComparer<T> : IEqualityComparer<ReadOnlyMemory<T>> 
    where T : IEquatable<T>
{
    public static readonly ReadOnlyMemoryEqualityComparer<T> Instance = new();

    public bool Equals(ReadOnlyMemory<T> first, ReadOnlyMemory<T> second)
    {
        return first.Span.SequenceEqual(second.Span);
    }

    public int GetHashCode(ReadOnlyMemory<T> memory)
    {
        ReadOnlySpan<T> span = memory.Span;
        HashCode hash = new();
        foreach (ref readonly T item in span)
        {
            hash.Add(item);
        }
        return hash.ToHashCode();
    }
}