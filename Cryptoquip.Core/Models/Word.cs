using System.Runtime.InteropServices;

namespace Cryptoquip.Models;

public class Word
{
    public string Text { get; }
    public string Pattern { get; }
    public uint LetterMask { get; }
    public List<string> Matches { get; set; }
    public bool IsSolvable => Text.Any(char.IsAsciiLetterUpper) && !Text.Any(char.IsWhiteSpace);
    
    public Word(string text)
    {
        Text = text;
        Pattern = MakePattern(text);
        Matches = [];
        LetterMask = MakeTextLetterMask(text);
    }

    public static string MakePattern(string text)
    {
        return string.Create(text.Length, text, static (chars, source) =>
        {
            int patternDepth = 0;
            Span<char> patternMap = stackalloc char[26];
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (!char.IsAsciiLetterUpper(c))
                {
                    chars[i] = c;
                    continue;
                }

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
        });
    }

    private static uint MakeTextLetterMask(string text)
    {
        uint mask = 0;
        foreach (char c in text)
        {
            if (char.IsAsciiLetterUpper(c)) mask |= 1u << (c - 'A');
        }
        return mask;
    }

    public MatchRequirements GetMatchRequirements()
    {
        return MatchRequirements.Build(Text, Matches);
    }

    public int EnsureMatchRequirements(MatchRequirements requirements)
    {
        Span<string> matches = CollectionsMarshal.AsSpan(Matches);
        int write = 0;
        for (int read = 0; read < matches.Length; read++)
        {
            string match = matches[read];
            if (!requirements.Matches(Text, match)) continue;
            matches[write] = match;
            write++;
        }

        int removed = matches.Length - write;
        if (removed > 0) Matches.RemoveRange(write, removed);
        return removed;
    }
}