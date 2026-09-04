namespace Cryptoquip.Models;

public class MatchRequirementsArray : MatchRequirements
{
    private int _count = 0;
    
    private readonly bool[]?[] _requirements = new bool[26][];
    
    public override int Count => _count;
    
    protected override void RegisterMatch(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (l < 'A' || l > 'Z') continue;
            
            char m = match[i];
            RegisterMatch(l, m);
        }
    }

    private void RegisterMatch(char l, char m)
    {
        int i = l - 'A';
        if (_requirements[i] == null)
        {
            _requirements[i] = new bool[26];
            _count++;
        }
        _requirements[i][m - 'A'] = true;
    }

    public override bool Matches(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (l < 'A' || l > 'Z') continue;
            if (_requirements[l - 'A'] == null) continue;

            char m = match[i];
            if (_requirements[l - 'A'][m - 'A']) continue;
            
            return false;
        }
        return true;
    }
    
    public override void Clear()
    {
        _count = 0;
        for (int i = 0; i < _requirements.Length; i++)
        {
            _requirements[i] = null;
        }
    }
}