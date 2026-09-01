namespace Cryptoquip.Services;

public class DecoderRingArray : DecoderRingAbstract
{
    private char[] _cypher = Enumerable.Range(0, 26).Select(static _ => '-').ToArray();
    private bool[] _usedLetters = new bool[26];
    private int _solveCount;

    public override int SolveCount => _solveCount;

    public override void Put(char letter, char match)
    {
        if (char.IsLetter(letter))
        {
            int i = letter - 'A';
            _cypher[i] = match;
            
            i = match - 'A';
            _usedLetters[i] = true;
            
            _solveCount++;
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
            char match = _cypher[i];
            _cypher[i] = '-';
            i = match - 'A';
            _usedLetters[i] = false;
            _solveCount--;
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
        for (int i = 0; i < _usedLetters.Length; i++)
        {
            if (_usedLetters[i])
                yield return (char)('A' + i);
        }
    }
    
    public override bool UsedContains(char letter)
    {
        return _usedLetters[letter - 'A'];
    }
    
    public override void Clear()
    {
        for (int i = 0; i < _cypher.Length; i++)
            _cypher[i] = '-';
        Array.Clear(_usedLetters);
        _solveCount = 0;
        base.Clear();
    }

    public override DecoderRingAbstract Clone()
    {
        DecoderRingArray that = new()
        {
            _cypher = this._cypher.ToArray(),
            Hints = this.Hints.ToHashSet(),
            _usedLetters = this._usedLetters.ToArray(),
        };
        return that;
    }
}