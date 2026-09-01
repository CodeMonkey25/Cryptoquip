namespace Cryptoquip.Services;

public class DecoderRingDictionary : DecoderRing
{
    private Dictionary<char, char> _map = new();
    public override int SolveCount => _map.Count;

    public override void Put(char letter, char match)
    {
        if (char.IsAsciiLetterUpper(letter))
        {
            _map[letter] = match;
        }
    }
	
    public override char Get(char letter)
    {
        return !char.IsAsciiLetterUpper(letter) ? letter : _map.GetValueOrDefault(letter, '-');
    }

    public override void Remove(char letter)
    {
        _map.Remove(letter);
    }

    public override bool Contains(char letter)
    {
        return _map.ContainsKey(letter);
    }

    public override IEnumerable<char> GetUsedLetters()
    {
        return _map.Values;
    }
    
    public override bool UsedContains(char letter)
    {
        return _map.ContainsValue(letter);
    }

    public override void Clear()
    {
        _map.Clear();
        base.Clear();
    }

    public override DecoderRing Clone()
    {
        return new DecoderRingDictionary
        {
            _map = this._map.ToDictionary(static entry => entry.Key, static entry => entry.Value),
            Hints = this.Hints.ToHashSet()
        };
    }
}
