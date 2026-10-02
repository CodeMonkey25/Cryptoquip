using System.Buffers;
using Cryptoquip.Extensions;
using Cryptoquip.Services;

namespace Cryptoquip.Models;

public class Puzzle
{
    private static readonly SearchValues<char> ValidWordChars = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ'");
    private static readonly char[] TrimChars = ['.', ',', '!', '?', '"', ';', ':'];
    
    public string OriginalText { get; }
    public ReadOnlyMemory<char> Text { get; set; }

    public Puzzle(string text, DecoderRing ring)
    {
        OriginalText = text.ToUpper().Trim();
        Text = OriginalText.AsMemory();
        ring.Clear();
        
        int i = Text.Span.IndexOf("<HINT>:", StringComparison.Ordinal);
        if (i >= 0)
        {
            ReadOnlyMemory<char> hint = Text.Slice(i + 7, text.Length - i - 7);
            ring.LoadHints(hint);
            Text = Text.Slice(0, i);
        }
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
            .Distinct()
            .Select(static w => new string(w.Span));
    }
}