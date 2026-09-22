using System.Runtime.InteropServices;

namespace Cryptoquip.Models;

public class Word : IComparable<Word>
{
    public string Text { get; }
    public string Pattern { get; }
    public uint LetterMask { get; }
    public List<string> Matches { get; set; }
    public bool IsSolvable { get; }
    
    public Word(string text)
    {
        Text = text;
        Pattern = MakePattern(text);
        Matches = [];
        LetterMask = MakeTextLetterMask(text);
        IsSolvable = Text.Any(char.IsAsciiLetterUpper) && !Text.Any(char.IsWhiteSpace);
    }

    public static string MakePattern(string text)
    {
        Span<char> patternBuffer = text.Length <= WordList.MaxWordLength ? stackalloc char[text.Length] : new char[text.Length];
        Span<char> letterBuffer = stackalloc char[26];
        Span<int> touchedBuffer = stackalloc int[26];
        return MakePattern(text, patternBuffer, letterBuffer, touchedBuffer);
    }

    public static string MakePattern(string text, Span<char> patternBuffer, Span<char> letterBuffer, Span<int> touchedBuffer)
    {
        int patternDepth = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (!char.IsAsciiLetterUpper(c))
            {
                patternBuffer[i] = c;
                continue;
            }

            int patternIndex = c - 'A';
            char match = letterBuffer[patternIndex];
            if (match != '\0')
            {
                patternBuffer[i] = match;
                continue;
            }
            match = (char)('A' + patternDepth);
            touchedBuffer[patternDepth] = patternIndex;
            patternDepth++;
            letterBuffer[patternIndex] = match;
            patternBuffer[i] = match;
        }

        for (int i = 0; i < patternDepth; i++)
        {
            letterBuffer[touchedBuffer[i]] = '\0';
        }

        return new string(patternBuffer);
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

    public int CompareTo(Word? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        if (Text == other.Text) return 0;
        
        if (Matches.Count != other.Matches.Count) return Matches.Count.CompareTo(other.Matches.Count);
        return -Text.Length.CompareTo(other.Text.Length);
    }
}