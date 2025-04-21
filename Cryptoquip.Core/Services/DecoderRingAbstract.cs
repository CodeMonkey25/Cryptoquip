using Cryptoquip.Extensions;

namespace Cryptoquip.Services;

public abstract class DecoderRingAbstract
{
    protected HashSet<char> Hints = [];
    
    public abstract int SolveCount { get; }
    public abstract void Put(char letter, char match);
    public abstract char Get(char letter);
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
                if (!char.IsLetter(c1)) continue;
                if (!char.IsLetter(c2)) continue;
                Put(c1, c2);
                Hints.Add(c1);
            }
        }
    }

    public virtual string Decode(ReadOnlyMemory<char> message) => string.Concat(message.Select(Get));
    
    public abstract void Remove(char letter);
    public abstract bool Contains(char letter);
    public abstract bool UsedContains(char letter);

    public virtual void Clear()
    {
        Hints.Clear();
    }

    public abstract IEnumerable<char> GetUsedLetters();
    
    public virtual IEnumerable<char> GetUnusedLetters()
    {
        return Enumerable.Range(0, 26)
            .Select(static i => (char)('A' + i))
            .Except(GetUsedLetters())
            .ToArray();
    }
    
    public virtual bool WasSetFromHint(char letter)
    {
        return Hints.Contains(letter);
    }
    
    public abstract DecoderRingAbstract Clone();
}