namespace Cryptoquip.Services;

public class DecoderRingDictionary : DecoderRingAbstract
{
    private Dictionary<char, char> _map = new();
    public override int SolveCount => _map.Count;

    public override void Put(char letter, char match)
    {
        if (char.IsLetter(letter))
        {
            _map[letter] = match;
        }
    }
	
    public override char Get(char letter)
    {
        return !char.IsLetter(letter) ? letter : _map.GetValueOrDefault(letter, '-');
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
        return _map.Keys;
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

    public override DecoderRingAbstract Clone()
    {
        DecoderRingDictionary that = new()
        {
            _map = this._map.ToDictionary(static entry => entry.Key, static entry => entry.Value),
            _hints = this._hints.ToHashSet()
        };
        return that;
    }
}
