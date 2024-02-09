using System.Collections.Generic;
using System.Linq;

namespace Cryptoquip.Utility;

public class DecoderRing
{
    private Dictionary<char, char> _map = new Dictionary<char, char>();
    private ISet<char> _hints = new HashSet<char>();

    public void Put(char letter, char match)
    {
        _map[letter] = match;
    }
	
    public char Get(char letter)
    {
        return !char.IsLetter(letter) ? letter : _map.GetValueOrDefault(letter, '-');
    }

    public bool Matches(string encrypted, string candidate)
    {
        for (int i = 0; i < encrypted.Length; i++)
        {
            char letter = encrypted[i];
            if (!_map.TryGetValue(letter, out char ringMatch)) continue;
            char candidateMatch = candidate[i];
            if (ringMatch != candidateMatch)
            {
                return false;
            }
        }

        foreach ((char key, char value) in _map)
        {
            if (encrypted.Contains(key)) { continue; }
            if (candidate.Contains(value)) { return false; }
        }
		
        return true;
    }

    public void LoadHints(string hints)
    {
        foreach (string hint in hints.Split(",").Select(h => h.Trim()))
        {
            string[] parts = hint.Split("=").Select(h => h.Trim()).ToArray();
            if(parts.Length != 2) continue;
            if(parts[0].Length != parts[1].Length) continue;

            foreach ((char c1, char c2) in parts[0].Zip(parts[1]))
            {
                if (!char.IsLetter(c1)) continue;
                if (!char.IsLetter(c2)) continue;
                Put(c1, c2);
                _hints.Add(c1);
            }
        }
    }

    internal string Decode(string message)
    {
        char[] chars = message.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = Get(chars[i]);
        }
        return new string(chars);
    }

    internal void Remove(char letter)
    {
        _map.Remove(letter);
    }

    internal bool Contains(char letter)
    {
        return _map.ContainsKey(letter);
    }

    public void Clear()
    {
        _map.Clear();
        _hints.Clear();
    }

    public IEnumerable<char> GetUsedLetters()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        foreach (char c in alphabet)
        {
            if (_map.ContainsValue(c)) continue;
            yield return c;
        }
    }

    public bool WasSetFromHint(char letter)
    {
        return _hints.Contains(letter);
    }
}
