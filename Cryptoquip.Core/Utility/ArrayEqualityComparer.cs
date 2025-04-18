namespace Cryptoquip.Utility;

public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]> where T : notnull
{
    private static readonly EqualityComparer<T> ElementComparer = EqualityComparer<T>.Default;

    public bool Equals(T[]? first, T[]? second)
    {
        if (first == second) return true;
        if (first == null || second == null) return false;
        if (first.Length != second.Length) return false;
            
        for (int i = 0; i < first.Length; i++)
        {
            if (!ElementComparer.Equals(first[i], second[i]))
            {
                return false;
            }
        }
        return true;
    }

    public int GetHashCode(T[]? array)
    {
        if (array == null) return 0;
        unchecked
        {
            int hash = 17;
            foreach (T element in array)
            {
                hash = hash * 31 + ElementComparer.GetHashCode(element);
            }
            return hash;
        }
    }
}
