using System.Linq;

namespace Cryptoquip.Utility;

public class DecoderRingCustom : DecoderRingAbstract
{
    private char[] _cypher = Enumerable.Range(0, 26).Select(_ => '-').ToArray();
    public override int SolveCount => _cypher.Length - _cypher.Count(c => c != '-');

    public override void Put(char letter, char match)
    {
        if (char.IsLetter(letter))
        {
            int i = letter - 'A';
            _cypher[i] = match;
        }
    }

    public override char Get(char letter)
    {
        if (char.IsLetter(letter))
        {
            int i = letter - 'A';
            return _cypher[i];
        }

        return letter;
    }

    public override void Remove(char letter)
    {
        if (char.IsLetter(letter))
        {
            int i = letter - 'A';
            _cypher[i] = '-';
        }
    }

    public override bool Contains(char letter)
    {
        if (char.IsLetter(letter))
        {
            int i = letter - 'A';
            return _cypher[i] != '-';
        }

        return false;
    }

    public override bool UsedContains(char letter)
    {
        return _cypher.Contains(letter);
    }
    
    public override void Clear()
    {
        for (int i = 0; i < _cypher.Length; i++)
            _cypher[i] = '-';
        base.Clear();
    }

    public override DecoderRingAbstract Clone()
    {
        DecoderRingCustom that = new DecoderRingCustom();
        that._cypher = this._cypher.ToArray();
        that._hints = this._hints.ToHashSet();
        return that;
    }
}