namespace Cryptoquip.Extensions;

public static class ReadOnlyMemoryExtensions
{
    public static IEnumerable<ReadOnlyMemory<char>> Split(this ReadOnlyMemory<char> source, char splitChar)
    {
        int start = 0;
        for (int i = 0; i < source.Length; i++)
        {
            char c = source.Span[i];
            if (c != splitChar) continue;
            yield return source.Slice(start, i - start);
            start = i + 1;
        }
		
        if (start < source.Length)
        {
            yield return source.Slice(start);
        }
    }
	
    public static bool Any(this ReadOnlyMemory<char> source, Predicate<char> predicate)
    {
        foreach(char c in source.Span)
        {
            if (!predicate(c))
                return true;
        }
        return false;
    }
	
    public static bool All(this ReadOnlyMemory<char> source, Predicate<char> predicate)
    {
        foreach(char c in source.Span)
        {
            if (!predicate(c))
                return false;
        }
        return true;
    }

    public static IEnumerable<ReadOnlyMemory<T>> Prepend<T>(this IEnumerable<ReadOnlyMemory<T>> source, ReadOnlyMemory<T> value)
    {
        yield return value;
        foreach (ReadOnlyMemory<T> item in source)
        {
            yield return item;
        }
    }
    
    public static IEnumerable<ReadOnlyMemory<T>> Append<T>(this IEnumerable<ReadOnlyMemory<T>> source, ReadOnlyMemory<T> value)
    {
        foreach (ReadOnlyMemory<T> item in source)
        {
            yield return item;
        }
        yield return value;
    }
    
    public static IEnumerable<TResult> Select<T, TResult>(this ReadOnlyMemory<T> source, Func<T, TResult> selector)
    {
        for (int i = 0; i < source.Span.Length; i++)
        {
            T item = source.Span[i];
            yield return selector(item);
        }
    }
}