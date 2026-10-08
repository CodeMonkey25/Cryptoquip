namespace Cryptoquip.Models;

public sealed class MatchRequirementsDictionary : MatchRequirements
{
    private readonly Dictionary<char, HashSet<char>> _requirements = new();

    protected override void RegisterMatch(ReadOnlySpan<char> text, ReadOnlySpan<char> match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (!char.IsAsciiLetterUpper(l)) continue;

            char m = match[i];
            if (!char.IsAsciiLetterUpper(m)) continue;

            if (_requirements.TryGetValue(l, out HashSet<char>? set))
            {
                set.Add(m);
            }
            else
            {
                _requirements[l] = [m];
            }
        }
    }

    public override bool Matches(ReadOnlySpan<char> text, ReadOnlySpan<char> match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (!char.IsAsciiLetterUpper(l)) continue;
            if (!_requirements.TryGetValue(l, out HashSet<char>? set)) continue;

            char m = match[i];
            if (set.Contains(m)) continue;
            
            return false;
        }
        return true;
    }
    
    public override void Clear()
    {
        _requirements.Clear();
    }
}