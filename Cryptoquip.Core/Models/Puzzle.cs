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

    public string[] GetWords()
    {
        return Text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    }
}