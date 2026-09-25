using Cryptoquip.Extensions;

namespace Cryptoquip.Services;

public abstract class DecoderRing
{
    protected internal HashSet<char> Hints = [];
    
    public abstract int SolveCount { get; }
    public abstract char Get(char letter);
    public abstract IEnumerable<(char letter, char match)> GetMatches();
    public abstract void Put(char letter, char match);

    public int Put(string letters, string matches, Span<char> addedLetters = default)
    {
        int count = 0;
        for (int i = 0; i < letters.Length; i++)
        {
            if (!char.IsAsciiLetterUpper(letters[i])) continue;
            if (Contains(letters[i])) continue;
            Put(letters[i], matches[i]);
            if (!addedLetters.IsEmpty) addedLetters[count] = letters[i];
            count++;
        }
        return count;
    }
    
    public virtual bool Matches(string encrypted, string candidate)
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

    public virtual void LoadHints(ReadOnlyMemory<char> hints)
    {
        foreach (ReadOnlyMemory<char> hint in hints.Split(',').Select(static h => h.Trim()))
        {
            ReadOnlyMemory<char>[] parts = hint.Split('=').Select(static h => h.Trim()).ToArray();
            if (parts.Length != 2) continue;
            if (parts[0].Length != parts[1].Length) continue;

            for(int i = 0; i < parts[0].Length; i++)
            {
                char c1 = parts[0].Span[i];
                char c2 = parts[1].Span[i];
                if (!char.IsAsciiLetterUpper(c1)) continue;
                if (!char.IsAsciiLetterUpper(c2)) continue;
                Put(c1, c2);
                Hints.Add(c1);
            }
        }
    }

    public virtual string Decode(ReadOnlyMemory<char> message) => string.Concat(message.Select(Get));
    
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
        Hints.Clear();
    }

    public abstract IEnumerable<char> GetUsedLetters();
    
    public virtual IEnumerable<char> GetUnusedLetters()
    {
        bool[] used = new bool[26];
        foreach (char c in GetUsedLetters())
            used[c - 'A'] = true;

        for (int i = 0; i < 26; i++)
            if (!used[i]) yield return (char)('A' + i);
    }
    
    public virtual bool WasSetFromHint(char letter)
    {
        return Hints.Contains(letter);
    }
    
    public abstract DecoderRing Clone();
    
    public virtual void Overwrite(DecoderRing other)
    {
        Clear();
        foreach (var (letter, match) in other.GetMatches())
        {
            Put(letter, match);
        }
        foreach (char hint in other.Hints)
        {
            Hints.Add(hint);
        }
    }
}