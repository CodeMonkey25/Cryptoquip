namespace Cryptoquip.Models;

public abstract class MatchRequirements
{
    public abstract int Count { get; }

    protected MatchRequirements() { }
   
    public static MatchRequirements Build(string text, IEnumerable<string> matches)
    {
        MatchRequirements requirements = new MatchRequirementsBitmask();
        requirements.Rebuild(text, matches);
        return requirements;
    }
    
    public void Rebuild(string text, IEnumerable<string> matches)
    {
        Clear();
        foreach (string match in matches)
        {
            RegisterMatch(text, match);
        }
    }
    
    protected abstract void RegisterMatch(string text, string match);
    
    public abstract bool Matches(string text, string match);
    
    public abstract void Clear();
}