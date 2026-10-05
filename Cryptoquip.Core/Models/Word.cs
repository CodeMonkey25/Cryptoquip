using System.Diagnostics;
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
        Pattern = Word.MakePattern(Text);
        Matches = [];
        LetterMask = Text.Where(char.IsAsciiLetterUpper).Aggregate(0u, (mask, c) => mask | 1u << (c - 'A'));
        IsSolvable = LetterMask != 0u && !Text.Any(char.IsWhiteSpace);
    }

    public static string MakePattern(string text)
    {
        Span<char> patternBuffer = text.Length <= WordList.MaxWordLength ? stackalloc char[text.Length] : new char[text.Length];
        Span<char> letterBuffer = stackalloc char[26];
        Span<int> touchedBuffer = stackalloc int[26];
        return Word.MakePattern(text, patternBuffer, letterBuffer, touchedBuffer);
    }

    /// <remarks>letterBuffer must be zeroed on entry; it is restored to zero on exit.</remarks>
    public static string MakePattern(string text, Span<char> patternBuffer, Span<char> letterBuffer, Span<int> touchedBuffer)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(patternBuffer.Length, text.Length);
        ArgumentOutOfRangeException.ThrowIfLessThan(letterBuffer.Length, 26);
        ArgumentOutOfRangeException.ThrowIfLessThan(touchedBuffer.Length, 26);
        Word.WritePattern(text.AsSpan(), patternBuffer, letterBuffer, touchedBuffer);
        return new string(patternBuffer.Slice(0, text.Length));
    }

    /// <remarks>letterBuffer must be zeroed on entry; it is restored to zero on exit.</remarks>
    public static void WritePattern(ReadOnlySpan<char> text, Span<char> patternBuffer, Span<char> letterBuffer, Span<int> touchedBuffer)
    {
        Debug.Assert(patternBuffer.Length >= text.Length);
        Debug.Assert(letterBuffer.Length >= 26 && touchedBuffer.Length >= 26);
        
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
        if (Text.Length != other.Text.Length) return -Text.Length.CompareTo(other.Text.Length);
        return string.Compare(Text, other.Text, StringComparison.Ordinal);
    }
}