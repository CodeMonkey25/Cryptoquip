using System.Buffers;
using Cryptoquip.Extensions;
using Cryptoquip.Utility;

namespace Cryptoquip.Models;

public class Puzzle
{
    private static readonly SearchValues<char> ValidWordChars = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ'");
    private static readonly char[] TrimChars = ['.', ',', '!', '?', '"', ';', ':',];

    public string OriginalText { get; }
    public ReadOnlyMemory<char> Text { get; }
    public IReadOnlyDictionary<char, char> Hints { get; }

    private Puzzle(string originalText, ReadOnlyMemory<char> text, IReadOnlyDictionary<char, char> hints)
    {
        OriginalText = originalText;
        Text = text;
        Hints = hints;
    }
    
    public static Puzzle Parse(string puzzle)
    {
        string originalText = puzzle.Trim().ToUpperInvariant();
        ReadOnlyMemory<char> text = originalText.AsMemory();
        Dictionary<char, char> hints = new();
        
        int i = originalText.IndexOf("<HINT>:", StringComparison.Ordinal);
        if (i >= 0)
        {
            hints = ParseHints(originalText.AsSpan(i + 7));
            text = text.Slice(0, i).TrimEnd();
        }
        
        return new Puzzle(originalText, text, hints);
    }
    
    private static Dictionary<char, char> ParseHints(ReadOnlySpan<char> hints)
    {
        Dictionary<char, char> hintsDict = new();
    
        foreach (Range hintRange in hints.Split(','))
        {
            ReadOnlySpan<char> hint = hints[hintRange].Trim();
            int eqIndex = hint.IndexOf('=');
            if (eqIndex <= 0 || eqIndex == hint.Length - 1) continue;

            ReadOnlySpan<char> word = hint.Slice(0, eqIndex).Trim();
            ReadOnlySpan<char> match = hint.Slice(eqIndex + 1).Trim();

            if (word.Length != match.Length) continue;

            for (int j = 0; j < word.Length; j++)
            {
                char c1 = word[j];
                char c2 = match[j];
                if (char.IsAsciiLetterUpper(c1) && char.IsAsciiLetterUpper(c2))
                {
                    hintsDict[c1] = c2;
                }
            }
        }
        return hintsDict;
    }
    
    public IEnumerable<ReadOnlyMemory<char>> GetAllWords()
    {
        return Text
            .Split(' ')
            .Select(static w => w.Trim())
            .Where(static w => !w.IsEmpty);
    }

    public IEnumerable<string> GetFilteredAndDistinctWords()
    {
        return GetAllWords()
            .Select(static w => w.Trim(Puzzle.TrimChars))
            .Where(static w => !w.IsEmpty)
            .Where(static w => !w.Span.ContainsAnyExcept(Puzzle.ValidWordChars))
            .Distinct(ReadOnlyMemoryEqualityComparer<char>.Instance)
            .Select(static w => new string(w.Span));
    }
}