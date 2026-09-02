using Cryptoquip.Extensions;
using Cryptoquip.Services;

namespace Cryptoquip.Models;

public class Puzzle
{
    public string OriginalText { get; }
    public ReadOnlyMemory<char> Text { get; set; }

    public Puzzle(string text, DecoderRing ring)
    {
        OriginalText = text.ToUpper().Trim();
        Text = text.ToUpper().Trim().AsMemory();
        ring.Clear();
        
        int i = text.IndexOf("<HINT>:", StringComparison.Ordinal);
        if (i >= 0)
        {
            ReadOnlyMemory<char> hint = Text.Slice(i + 7, text.Length - i - 7);
            ring.LoadHints(hint);
            Text = Text.Slice(0, i);
        }
    }

    public string[] GetAllWords()
    {
        return Text
            .Split(' ')
            .Select(static w => w.Trim())
            .Select(static w => new string(w.Span))
            .Where(w => !string.IsNullOrEmpty(w))
            .ToArray();
    }

    public string[] GetFilteredAndDistinctWords()
    {
        return GetAllWords()
            .Select(static w => w.Trim('.'))
            .Select(static w => w.Trim(','))
            .Select(static w => w.Trim('!'))
            .Select(static w => w.Trim('?'))
            .Select(static w => w.Trim('"'))
            .Where(static w => w.All(static c => char.IsAsciiLetterUpper(c) || c == '\''))
            .Distinct()
            .ToArray();
    }
}