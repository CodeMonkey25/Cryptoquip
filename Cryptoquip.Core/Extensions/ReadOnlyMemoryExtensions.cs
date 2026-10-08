using System.Buffers;

namespace Cryptoquip.Extensions;

public static class ReadOnlyMemoryExtensions
{
    private const int StackAllocThreshold = 128;

    public static IEnumerable<ReadOnlyMemory<char>> Split(this ReadOnlyMemory<char> source, char splitChar, StringSplitOptions options = StringSplitOptions.None)
    {
        int separatorCount = source.Span.Count(splitChar);
        int maxRanges = separatorCount + 1;

        Range[]? rented = null;
        Span<Range> ranges = maxRanges <= StackAllocThreshold
            ? stackalloc Range[maxRanges]
            : (rented = ArrayPool<Range>.Shared.Rent(maxRanges));

        try
        {
            int count = source.Span.Split(ranges, splitChar, options);
            var result = new ReadOnlyMemory<char>[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = source[ranges[i]];
            }
            return result;
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<Range>.Shared.Return(rented);
            }
        }
    }

    public static IEnumerable<ReadOnlyMemory<char>> Split(this ReadOnlyMemory<char> source, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None)
    {
        int separatorCount = source.Span.Count(separator);
        int maxRanges = separatorCount + 1;

        Range[]? rented = null;
        Span<Range> ranges = maxRanges <= StackAllocThreshold
            ? stackalloc Range[maxRanges]
            : (rented = ArrayPool<Range>.Shared.Rent(maxRanges));

        try
        {
            int count = source.Span.Split(ranges, separator, options);
            var result = new ReadOnlyMemory<char>[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = source[ranges[i]];
            }
            return result;
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<Range>.Shared.Return(rented);
            }
        }
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

    public static IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this ReadOnlyMemory<TFirst> first, ReadOnlyMemory<TSecond> second)
    {
        int minLength = Math.Min(first.Length, second.Length);
        for (int i = 0; i < minLength; i++)
        {
            yield return (first.Span[i], second.Span[i]);
        }
    }

    public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this ReadOnlyMemory<TFirst> first, ReadOnlyMemory<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        int minLength = Math.Min(first.Length, second.Length);
        for (int i = 0; i < minLength; i++)
        {
            yield return resultSelector(first.Span[i], second.Span[i]);
        }
    }
}