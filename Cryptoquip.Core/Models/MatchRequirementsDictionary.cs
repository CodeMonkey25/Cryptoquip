namespace Cryptoquip.Models;

public class MatchRequirementsDictionary : MatchRequirements
{
    private readonly Dictionary<char, HashSet<char>> _requirements = new();
    
    public override int Count => _requirements.Count;

    protected override void RegisterMatch(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            char m = match[i];
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

    public override bool Matches(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
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