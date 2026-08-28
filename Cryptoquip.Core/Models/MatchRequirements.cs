namespace Cryptoquip.Models;

public abstract class MatchRequirements
{
    public abstract int Count { get; }

    protected MatchRequirements() { }
   
    public static MatchRequirements Build(string text, IEnumerable<string> matches)
    {
        MatchRequirements requirements = new MatchRequirementsArray();
        foreach (string match in matches)
        {
            requirements.RegisterMatch(text, match);
        }
        return requirements;
    }
    
    protected abstract void RegisterMatch(string text, string match);
    
    public abstract bool Matches(string text, string match);
}