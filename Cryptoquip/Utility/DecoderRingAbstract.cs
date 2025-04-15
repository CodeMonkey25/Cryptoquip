using System.Collections.Generic;
using System.Linq;

namespace Cryptoquip.Utility;

public abstract class DecoderRingAbstract
{
    protected HashSet<char> _hints = [];
    
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

    public virtual void LoadHints(string hints)
    {
        foreach (string hint in hints.Split(",").Select(static h => h.Trim()))
        {
            string[] parts = hint.Split("=").Select(static h => h.Trim()).ToArray();
            if (parts.Length != 2) continue;
            if (parts[0].Length != parts[1].Length) continue;

            foreach ((char c1, char c2) in parts[0].Zip(parts[1]))
            {
                if (!char.IsLetter(c1)) continue;
                if (!char.IsLetter(c2)) continue;
                Put(c1, c2);
                _hints.Add(c1);
            }
        }
    }

    public virtual string Decode(string message) => string.Concat(message.Select(Get));
    
    public abstract void Remove(char letter);
    public abstract bool Contains(char letter);
    public abstract bool UsedContains(char letter);

    public virtual void Clear()
    {
        _hints.Clear();
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
        return _hints.Contains(letter);
    }
    
    public abstract DecoderRingAbstract Clone();
}