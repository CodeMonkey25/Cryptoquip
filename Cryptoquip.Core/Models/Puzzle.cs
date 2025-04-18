using Cryptoquip.Services;

namespace Cryptoquip.Models;

public class Puzzle
{
    public string OriginalText { get; }
    public string Text { get; set; }

    public Puzzle(string text, DecoderRingAbstract ring)
    {
        OriginalText = text;
        ring.Clear();
        
        string[] input = text.ToUpper().Split("<HINT>:");
        Text = input.First();
        if (input.Length >= 2)
        {
            ring.LoadHints(input[1]);
        }
    }

    public string[] GetAllWords()
    {
        return Text
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(static w => w.Trim())
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