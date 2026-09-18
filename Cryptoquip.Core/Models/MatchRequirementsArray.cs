namespace Cryptoquip.Models;

public sealed class MatchRequirementsArray : MatchRequirements
{
    private readonly bool[]?[] _requirements = new bool[26][];
    
    protected override void RegisterMatch(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (!char.IsAsciiLetterUpper(l)) continue;
            
            char m = match[i];
            if (!char.IsAsciiLetterUpper(m)) continue;
            RegisterMatch(l, m);
        }
    }

    private void RegisterMatch(char l, char m)
    {
        int i = l - 'A';
        if (_requirements[i] == null)
        {
            _requirements[i] = new bool[26];
        }
        _requirements[i][m - 'A'] = true;
    }

    public override bool Matches(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (!char.IsAsciiLetterUpper(l)) continue;
            if (_requirements[l - 'A'] == null) continue;

            char m = match[i];
            if (!char.IsAsciiLetterUpper(m)) return false;
            if (_requirements[l - 'A'][m - 'A']) continue;
            
            return false;
        }
        return true;
    }
    
    public override void Clear()
    {
        for (int i = 0; i < _requirements.Length; i++)
        {
            _requirements[i] = null;
        }
    }
}