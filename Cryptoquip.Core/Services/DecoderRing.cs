using Cryptoquip.Extensions;

namespace Cryptoquip.Services;

public abstract class DecoderRing
{
    protected internal uint Hints { get; protected set; }
    
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

    public void LoadHints(ReadOnlyMemory<char> hints)
    {
        foreach (ReadOnlyMemory<char> hint in hints.Split(',').Select(static h => h.Trim()))
        {
            ReadOnlyMemory<char>[] parts = hint.Split('=').Select(static h => h.Trim()).ToArray();
            if (parts.Length != 2) continue;
            
            (ReadOnlyMemory<char> word, ReadOnlyMemory<char> match) = (parts[0], parts[1]);
            if (word.Length != match.Length) continue;
            
            foreach ((char c1, char c2) in word.Zip(match))
            {
                if (Put(c1, c2)) AddHint(c1);
            }
        }
    }

    public void AddHint(char letter)
    {
        if (char.IsAsciiLetterUpper(letter)) Hints |= 1u << (letter - 'A');
    }

    public bool WasSetFromHint(char letter) => char.IsAsciiLetterUpper(letter) && (Hints & (1u << (letter - 'A'))) != 0;

    public string Decode(ReadOnlyMemory<char> message) => string.Concat(message.Select(Get));
    
    public abstract void Remove(char letter);

    public virtual void Remove(Span<char> letters)
    {
        foreach (char letter in letters)
        {
            Remove(letter);
        }
    }
    
    public abstract bool Contains(char letter);
    public abstract bool UsedContains(char letter);

    public virtual void Clear()
    {
        Hints = 0;
    }

    public abstract IEnumerable<char> GetUsedLetters();
    
    public virtual IEnumerable<char> GetUnusedLetters()
    {
        uint used = 0;
        foreach (char c in GetUsedLetters()) used |= 1u << (c - 'A');
        for (int i = 0; i < 26; i++)
            if ((used & (1u << i)) == 0) yield return (char)('A' + i);
    }

    public abstract DecoderRing Clone();
    
    public virtual void Overwrite(DecoderRing other)
    {
        Clear();
        foreach (var (letter, match) in other.GetMatches())
        {
            Put(letter, match);
        }
        Hints = other.Hints;
    }
}