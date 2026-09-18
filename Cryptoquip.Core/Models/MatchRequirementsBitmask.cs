namespace Cryptoquip.Models;

public sealed class MatchRequirementsBitmask : MatchRequirements
{
    // Bit i (0..25) is set if ('A' + i) is an allowed plain character
    private readonly uint[] _allowedMasks = new uint[26];

    protected override void RegisterMatch(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            char m = match[i];
            if (char.IsAsciiLetterUpper(l) && char.IsAsciiLetterUpper(m))
            {
                int index = l - 'A';
                _allowedMasks[index] |= 1u << (m - 'A');
            }
        }
    }

    public override bool Matches(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (!char.IsAsciiLetterUpper(l)) continue;

            uint mask = _allowedMasks[l - 'A'];
            if (mask == 0) continue; // No constraint on this letter

            char m = match[i];
            if (!char.IsAsciiLetterUpper(m) || (mask & (1u << (m - 'A'))) == 0)
                return false;
        }
        return true;
    }
    
    public override void Clear()
    {
        Array.Clear(_allowedMasks);
    }
}