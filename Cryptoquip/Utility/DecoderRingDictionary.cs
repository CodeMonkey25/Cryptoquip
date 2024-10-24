using System.Collections.Generic;
using System.Linq;

namespace Cryptoquip.Utility;

public class DecoderRingDictionary : DecoderRingAbstract
{
    private Dictionary<char, char> _map = new Dictionary<char, char>();
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
        DecoderRingDictionary that = new DecoderRingDictionary();
        that._map = this._map.ToDictionary(entry => entry.Key, entry => entry.Value);
        that._hints = this._hints.ToHashSet();
        return that;
    }
}
