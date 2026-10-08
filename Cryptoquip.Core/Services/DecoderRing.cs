using Cryptoquip.Extensions;

namespace Cryptoquip.Services;

public abstract class DecoderRing
{
    public static DecoderRing Build() => new DecoderRingBitmask();
    
    public abstract int SolveCount { get; }
    public abstract char Get(char letter);
    public abstract IEnumerable<(char letter, char match)> GetMatches();
    public abstract bool Put(char letter, char match);

    public int Put(ReadOnlySpan<char> letters, ReadOnlySpan<char> matches, Span<char> addedLetters = default)
    {
        int count = 0;
        bool trackAdded = !addedLetters.IsEmpty;
        for (int i = 0; i < letters.Length; i++)
        {
            bool added = Put(letters[i], matches[i]);
            if (!added) continue;
            if (trackAdded) addedLetters[count] = letters[i];
            count++;
        }
        return count;
    }

    public void Put(IReadOnlyDictionary<char, char> hints)
    {
        foreach ((char letter, char match) in hints)
        {
            Put(letter, match);
        }
    }
    
    public virtual bool Matches(ReadOnlySpan<char> encrypted, ReadOnlySpan<char> candidate)
    {
        for (int i = 0; i < encrypted.Length; i++)
        {
            char letter = encrypted[i];
            char ringMatch = Get(letter);
            char candidateMatch = candidate[i];
            if (ringMatch != '-')
            {
                if (ringMatch != candidateMatch)
                {
                    return false;
                }
            }
            else
            {
                if (UsedContains(candidateMatch))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public string Decode(ReadOnlyMemory<char> message) => string.Concat(message.Select(Get));
    
    public abstract void Remove(char letter);

    public void Remove(Span<char> letters)
    {
        foreach (char letter in letters)
        {
            Remove(letter);
        }
    }
    
    public abstract bool Contains(char letter);
    public abstract bool UsedContains(char letter);

    public abstract void Clear();

    public abstract IEnumerable<char> GetUsedLetters();
    
    public virtual IEnumerable<char> GetUnusedLetters()
    {
        uint used = GetUsedLetters().Aggregate<char, uint>(0, (current, c) => current | 1u << (c - 'A'));

        for (int i = 0; i < 26; i++)
        {
            if ((used & (1u << i)) == 0)
            {
                yield return (char)('A' + i);
            }
        }
    }

    public abstract DecoderRing Clone();
    
    public virtual void Overwrite(DecoderRing other)
    {
        Clear();
        foreach (var (letter, match) in other.GetMatches())
        {
            Put(letter, match);
        }
    }
}