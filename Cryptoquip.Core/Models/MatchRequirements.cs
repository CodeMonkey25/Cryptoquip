namespace Cryptoquip.Models;

public abstract class MatchRequirements
{
    public static MatchRequirements Build() => new MatchRequirementsBitmask();
    
    public static MatchRequirements Build(string text, List<string> matches)
    {
        MatchRequirements requirements = MatchRequirements.Build();
        requirements.Rebuild(text, matches);
        return requirements;
    }
    
    public void Rebuild(string text, List<string> matches)
    {
        Clear();
        for (int i = 0; i < matches.Count; i++)
            RegisterMatch(text, matches[i]);
    }
    
    protected abstract void RegisterMatch(string text, string match);
    
    public abstract bool Matches(string text, string match);
    
    public abstract void Clear();
}