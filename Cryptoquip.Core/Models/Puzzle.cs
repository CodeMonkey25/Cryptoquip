using System.Buffers;
using Cryptoquip.Extensions;
using Cryptoquip.Services;
using Cryptoquip.Utility;

namespace Cryptoquip.Models;

public class Puzzle
{
    private static readonly SearchValues<char> ValidWordChars = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ'");
    private static readonly char[] TrimChars = ['.', ',', '!', '?', '"', ';', ':'];

    public string OriginalText { get; }
    public ReadOnlyMemory<char> Text { get; }

    private Puzzle(string originalText, ReadOnlyMemory<char> text)
    {
        OriginalText = originalText;
        Text = text;
    }
    
    public static (Puzzle, DecoderRing) Parse(string text, DecoderRing? ring = null)
    {
        if (ring == null)
        {
            ring = DecoderRing.Build();
        }
        else
        {
            ring.Clear();
        }

        string originalText = text.ToUpper().Trim();
        ReadOnlyMemory<char> textMemory = originalText.AsMemory();
        int i = originalText.IndexOf("<HINT>:", StringComparison.Ordinal);
        if (i >= 0)
        {
            string hint = originalText.Substring(i + 7);
            ring.LoadHints(hint);
            textMemory = textMemory.Slice(0, i).TrimEnd();
        }
        
        return (new Puzzle(originalText, textMemory), ring);
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