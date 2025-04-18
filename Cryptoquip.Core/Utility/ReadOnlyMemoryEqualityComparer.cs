namespace Cryptoquip.Utility;

public class ReadOnlyMemoryEqualityComparer<T> : IEqualityComparer<ReadOnlyMemory<T>> where T : notnull
{
    private static readonly EqualityComparer<T> ElementComparer = EqualityComparer<T>.Default;

    public bool Equals(ReadOnlyMemory<T> first, ReadOnlyMemory<T> second)
    {
        if (first.Length != second.Length) return false;
            
        for (int i = 0; i < first.Length; i++)
        {
            if (!ElementComparer.Equals(first.Span[i], second.Span[i]))
            {
                return false;
            }
        }
        return true;
    }

    public int GetHashCode(ReadOnlyMemory<T> array)
    {
        unchecked
        {
            int hash = 17;
            foreach (T element in array.Span)
            {
                hash = hash * 31 + ElementComparer.GetHashCode(element);
            }
            return hash;
        }
    }
}