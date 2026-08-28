namespace Cryptoquip.Models;

public sealed class MatchRequirementsBitmask : MatchRequirements
{
    // Bit i (0..25) is set if ('A' + i) is an allowed plain character
    private readonly uint[] _allowedMasks = new uint[26];
    private int _count;

    public override int Count => _count;

    protected override void RegisterMatch(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            char m = match[i];
            if (l is >= 'A' and <= 'Z' && m is >= 'A' and <= 'Z')
            {
                int index = l - 'A';
                if (_allowedMasks[index] == 0) _count++;
                _allowedMasks[index] |= 1u << (m - 'A');
            }
        }
    }

    public override bool Matches(string text, string match)
    {
        for (int i = 0; i < match.Length; i++)
        {
            char l = text[i];
            if (l is < 'A' or > 'Z') continue;

            uint mask = _allowedMasks[l - 'A'];
            if (mask == 0) continue; // No constraint on this letter

            char m = match[i];
            if (m is < 'A' or > 'Z' || (mask & (1u << (m - 'A'))) == 0)
                return false;
        }
        return true;
    }
}