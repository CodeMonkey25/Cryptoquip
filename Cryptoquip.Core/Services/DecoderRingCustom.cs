namespace Cryptoquip.Services;

public class DecoderRingCustom : DecoderRingAbstract
{
    private char[] _cypher = Enumerable.Range(0, 26).Select(static _ => '-').ToArray();
    private HashSet<char> _usedLetters = new();

    public override int SolveCount => _usedLetters.Count;

    public override void Put(char letter, char match)
    {
        if (char.IsLetter(letter))
        {
            int i = letter - 'A';
            _cypher[i] = match;
            _usedLetters.Add(match);
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
            _usedLetters.Remove(_cypher[i]);
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

    public override IEnumerable<char> GetUsedLetters()
    {
        return _usedLetters;
    }
    
    public override bool UsedContains(char letter)
    {
        return _usedLetters.Contains(letter);
    }
    
    public override void Clear()
    {
        for (int i = 0; i < _cypher.Length; i++)
            _cypher[i] = '-';
        _usedLetters.Clear();
        base.Clear();
    }

    public override DecoderRingAbstract Clone()
    {
        DecoderRingCustom that = new()
        {
            _cypher = this._cypher.ToArray(),
            Hints = this.Hints.ToHashSet(),
            _usedLetters = this._usedLetters.ToHashSet(),
        };
        return that;
    }
}