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
        Matches = [];
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

    public Dictionary<char, HashSet<char>> GetMatchRequirements()
    {
        Dictionary<char, HashSet<char>> map = new();
        foreach (string match in Matches)
        {
            for (int i = 0; i < match.Length; i++)
            {
                char l = Text[i];
                char m = match[i];
                if (map.TryGetValue(l, out HashSet<char>? set))
                {
                    set.Add(m);
                }
                else
                {
                    map[l] = [m];
                }
            }
        }
        return map;
    }

    public void EnsureMatchRequirements(IReadOnlyDictionary<char, HashSet<char>> required)
    {
        Matches = Matches.Where(m => MatchesRequirements(m, required)).ToArray();
    }
    
    private bool MatchesRequirements(string match, IReadOnlyDictionary<char, HashSet<char>> required)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = Text[i];
            if (!required.TryGetValue(l, out HashSet<char>? set)) continue;

            char m = match[i];
            if (set.Contains(m)) continue;
            
            return false;
        }
        return true;
    }
}