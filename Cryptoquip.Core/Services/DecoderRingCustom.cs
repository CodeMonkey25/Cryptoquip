namespace Cryptoquip.Services;

public class DecoderRingCustom : DecoderRingAbstract
{
    private char[] _cypher = Enumerable.Range(0, 26).Select(static _ => '-').ToArray();
    public override int SolveCount => _cypher.Length - _cypher.Count(static c => c != '-');

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

    public override IEnumerable<char> GetUsedLetters()
    {
        return _cypher.Where(static c => c != '-').ToHashSet();
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
        DecoderRingCustom that = new()
        {
            _cypher = this._cypher.ToArray(),
            Hints = this.Hints.ToHashSet()
        };
        return that;
    }
}