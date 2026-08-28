namespace Cryptoquip.Models;

public class Word
{
    public string Text { get; }
    public string Pattern { get; }
    public List<string> Matches { get; set; }
    public bool IsSolvable => Text.Any(char.IsLetter) && !Text.Any(char.IsWhiteSpace);
    
    public Word(string text)
    {
        Text = text;
        Pattern = MakePattern(text);
        Matches = [];
    }

    public static string MakePattern(string text)
    {
        Span<char> patternMap = stackalloc char[26];
        int patternDepth = 0;
        char[] chars = new char[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (!char.IsLetter(c))
            {
                chars[i] = c;
                continue;
            }

            int patternIndex = c - 'A';
            char match = patternMap[patternIndex];
            if (match != '\0')
            {
                chars[i] = match;
                continue;
            }
            match = (char)('A' + patternDepth);
            patternDepth++;
            patternMap[patternIndex] = match;
            chars[i] = match;
        }
        return new string(chars);
    }

    public MatchRequirements GetMatchRequirements()
    {
        return MatchRequirements.Build(Text, Matches);
    }

    public void EnsureMatchRequirements(MatchRequirements requirements)
    {
        Matches.RemoveAll(match => !requirements.Matches(Text, match));
        
        // List<string> matches = [];
        // foreach (string match in Matches)
        // {
        //     if (requirements.Matches(Text, match))
        //     {
        //         matches.Add(match);
        //     }
        // }
        // Matches = matches;
    }
}