namespace Cryptoquip.Services;

public sealed class DecoderRingDictionary : IDecoderRing
{
    public static IDecoderRing Create() => new DecoderRingDictionary();
    
    private DecoderRingDictionary() { }

    private Dictionary<char, char> _map = new();
    public int SolveCount => _map.Count;
    
    public bool Put(char letter, char match)
    {
        if (!char.IsAsciiLetterUpper(letter)) return false;
        if (!char.IsAsciiLetterUpper(match)) return false;
        return _map.TryAdd(letter, match);
    }
	
    public char Get(char letter)
    {
        return !char.IsAsciiLetterUpper(letter) ? letter : _map.GetValueOrDefault(letter, '-');
    }

    public IEnumerable<(char letter, char match)> GetMatches()
    {
        for (char letter = 'A'; letter <= 'Z'; letter++)
            if (_map.TryGetValue(letter, out char match))
                yield return (letter, match);
    }

    public void Remove(char letter)
    {
        _map.Remove(letter);
    }

    public bool Contains(char letter)
    {
        return _map.ContainsKey(letter);
    }

    public IEnumerable<char> GetUsedLetters()
    {
        uint used = 0;
        foreach (char c in _map.Values) used |= 1u << (c - 'A');
        for (int i = 0; i < 26; i++)
            if ((used & (1u << i)) != 0) yield return (char)('A' + i);
    }
    
    public bool UsedContains(char letter)
    {
        return _map.ContainsValue(letter);
    }

    public void Clear()
    {
        _map.Clear();
    }

    public IDecoderRing Clone()
    {
        return new DecoderRingDictionary
        {
            _map = new(this._map),
        };
    }
}
