using System;
using System.Collections.Generic;
using System.Linq;

namespace Cryptoquip.Models;

public class Word
{
    public string Text { get; }
    public char[] Pattern { get; }
    public string[] Matches { get; set; }
    public bool IsSolvable => Text.Any(char.IsLetter) && !Text.Any(char.IsWhiteSpace);
    
    public Word(string text)
    {
        Text = text;
        Pattern = MakePattern(text);
        Matches = Array.Empty<string>();
    }

    public static char[] MakePattern(string text)
    {
        char[] patternMap = new char[26];
        int patternDepth = 0;
        char[] chars = new char[text.Length];
        for (int i = 0; i < chars.Length; i++)
        {
            char c = text[i];
            if (!char.IsLetter(c)) continue;

            int patternIndex = c - 'A';
            char match = patternMap[patternIndex];
            if (match != '\0')
            {
                chars[i] = match;
                continue;
            }
            match = (char)('A' + patternDepth);
            patternDepth++;
            patternMap[patternIndex] = match;
            chars[i] = match;
        }
        return chars;
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
        foreach ((char l, char m) in Text.Zip(match).Distinct())
        {
            if (!required.TryGetValue(l, out var set)) continue;
            if (set.Contains(m)) continue;
            return false;
        }
        return true;
    }
}