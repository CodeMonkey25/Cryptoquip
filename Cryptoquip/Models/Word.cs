using System.Collections.Generic;
using System.Linq;

namespace Cryptoquip.Models;

public class Word
{
    public string Text { get; }
    public string Pattern { get; }
    public bool IsSolvable => Text.Any(char.IsLetter) && !Text.Any(char.IsWhiteSpace);
    
    public Word(string text)
    {
        Text = text;
        Pattern = MakePattern(text);
    }

    static public string MakePattern(string text)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int currentLetterIndex = 0;
        Dictionary<char, char> seen = new Dictionary<char, char>();
        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!char.IsLetter(c)) continue;

            char match;
            if (seen.TryGetValue(c, out match))
            {
                chars[i] = match;
                continue;
            }
            match = alphabet[currentLetterIndex];
            currentLetterIndex++;
            seen.Add(chars[i], match);
            chars[i] = match;
        }
        return new string(chars);
    }
}