namespace Cryptoquip.Extensions;

public static class ReadOnlyMemoryExtensions
{
    public static IEnumerable<ReadOnlyMemory<char>> Split(this ReadOnlyMemory<char> source, char splitChar, StringSplitOptions options = StringSplitOptions.None)
    {
        int separatorCount = source.Span.Count(splitChar);
        Range[] ranges = new Range[separatorCount + 1];
        int count = source.Span.Split(ranges, splitChar, options);
        var result = new ReadOnlyMemory<char>[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = source[ranges[i]];
        }
        return result;
    }

    public static IEnumerable<ReadOnlyMemory<char>> Split(this ReadOnlyMemory<char> source, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
    {
        int separatorCount = source.Span.Count(separator);
        Range[] ranges = new Range[separatorCount + 1];
        int count = source.Span.Split(ranges, separator, options);
        var result = new ReadOnlyMemory<char>[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = source[ranges[i]];
        }
        return result;
    }

    public static bool Any<T>(this ReadOnlyMemory<T> source, Func<T, bool> predicate)
    {
        foreach (T item in source.Span)
        {
            if (predicate(item))
                return true;
        }
        return false;
    }

    public static bool Any(this ReadOnlyMemory<char> source, Predicate<char> predicate)
    {
        foreach (char c in source.Span)
        {
            if (predicate(c))
                return true;
        }
        return false;
    }

    public static bool All<T>(this ReadOnlyMemory<T> source, Func<T, bool> predicate)
    {
        foreach (T item in source.Span)
        {
            if (!predicate(item))
                return false;
        }
        return true;
    }

    public static bool All(this ReadOnlyMemory<char> source, Predicate<char> predicate)
    {
        foreach (char c in source.Span)
        {
            if (!predicate(c))
                return false;
        }
        return true;
    }

    public static IEnumerable<ReadOnlyMemory<T>> Prepend<T>(this IEnumerable<ReadOnlyMemory<T>> source, ReadOnlyMemory<T> value) =>
        Enumerable.Prepend(source, value);

    public static IEnumerable<ReadOnlyMemory<T>> Append<T>(this IEnumerable<ReadOnlyMemory<T>> source, ReadOnlyMemory<T> value) =>
        Enumerable.Append(source, value);

    public static IEnumerable<TResult> Select<T, TResult>(this ReadOnlyMemory<T> source, Func<T, TResult> selector)
    {
        for (int i = 0; i < source.Length; i++)
        {
            yield return selector(source.Span[i]);
        }
    }
}