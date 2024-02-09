using System;
using System.Collections.Generic;
using System.Linq;

namespace Cryptoquip.Models;

public class Word
{
    public string Text { get; }
    public string Pattern { get; }
    public string[] Matches { get; set; }
    public bool IsSolvable => Text.Any(char.IsLetter) && !Text.Any(char.IsWhiteSpace);
    
    public Word(string text)
    {
        Text = text;
        Pattern = MakePattern(text);
        Matches = Array.Empty<string>();
    }

    static public string MakePattern(string text)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int currentLetterIndex = 0;
        Dictionary<char, char> seen = new Dictionary<char, char>();
        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!char.IsLetter(c)) continue;

            char match;
            if (seen.TryGetValue(c, out match))
            {
                chars[i] = match;
                continue;
            }
            match = alphabet[currentLetterIndex];
            currentLetterIndex++;
            seen.Add(chars[i], match);
            chars[i] = match;
        }
        return new string(chars);
    }

    public Dictionary<char, ISet<char>> GetMatchRequirements()
    {
        Dictionary<char, ISet<char>> map = new();
        foreach (string match in Matches)
        {
            foreach ((char l, char m) in Text.Zip(match))
            {
                if (map.ContainsKey(l))
                {
                    map[l].Add(m);
                }
                else
                {
                    map[l] = new HashSet<char>(new[] { m });
                }
            }
        }
        return map;
    }

    public void EnsureMatchRequirements(IReadOnlyDictionary<char, ISet<char>> required)
    {
        Matches = Matches.Where(m => MatchesRequirements(m, required)).ToArray();
    }
    
    private bool MatchesRequirements(string match, IReadOnlyDictionary<char, ISet<char>> required)
    {
        foreach ((char l, char m) in Text.Zip(match))
        {
            if (!required.TryGetValue(l, out ISet<char> set)) continue;
            if (set.Contains(m)) continue;
            return false;
        }
        return true;
    }
}