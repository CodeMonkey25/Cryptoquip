using Cryptoquip.Extensions;
using Cryptoquip.Services;

namespace Cryptoquip.Models;

public class Puzzle
{
    public string OriginalText { get; }
    public ReadOnlyMemory<char> Text { get; set; }

    public Puzzle(string text, DecoderRingAbstract ring)
    {
        OriginalText = text;
        ring.Clear();
        
        int i = text.IndexOf("<HINT>:", StringComparison.Ordinal);
        if (i < 0)
        {
            Text = text.AsMemory();
        }
        else
        {
            Text = text.AsMemory(0, i);
            ReadOnlyMemory<char> hint = text.AsMemory(i + 7, text.Length - i - 7);
            ring.LoadHints(hint);
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
            .Where(static w => w.All(static c => char.IsLetter(c) || c == '\''))
            .Distinct()
            .ToArray();
    }
}