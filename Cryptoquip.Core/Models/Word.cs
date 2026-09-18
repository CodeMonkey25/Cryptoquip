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
        Pattern = MakePattern(text, new char[26], new int[26]);
        Matches = [];
        LetterMask = MakeTextLetterMask(text);
    }

    public static string MakePattern(string text, char[] letterBuffer, int[] touchedBuffer)
    {
        return string.Create(text.Length, (text, letterBuffer, touchedBuffer), static (chars, state) =>
        {
            var (source, map, touched) = state;
            int patternDepth = 0;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (!char.IsAsciiLetterUpper(c))
                {
                    chars[i] = c;
                    continue;
                }

                int patternIndex = c - 'A';
                char match = map[patternIndex];
                if (match != '\0')
                {
                    chars[i] = match;
                    continue;
                }
                match = (char)('A' + patternDepth);
                touched[patternDepth] = patternIndex;
                patternDepth++;
                map[patternIndex] = match;
                chars[i] = match;
            }

            for (int i = 0; i < patternDepth; i++)
            {
                map[touched[i]] = '\0';
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
        // perform in place compaction while removing unmatched words
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