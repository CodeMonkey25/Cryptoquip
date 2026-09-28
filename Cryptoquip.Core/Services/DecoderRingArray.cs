using System.Runtime.CompilerServices;

namespace Cryptoquip.Services;

public sealed class DecoderRingArray : DecoderRing
{
    private char[] _cypher = Enumerable.Range(0, 26).Select(static _ => '-').ToArray();
    private bool[] _usedLetters = new bool[26];
    private int _solveCount;

    public override int SolveCount => _solveCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Put(char letter, char match)
    {
        if (!char.IsAsciiLetterUpper(letter)) return false;
        if (!char.IsAsciiLetterUpper(match)) return false;
        
        int i = letter - 'A';
        if (_cypher[i] != '-') return false; // is letter already mapped?
        _cypher[i] = match;

        i = match - 'A';
        _usedLetters[i] = true;

        _solveCount++;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override char Get(char letter)
    {
        if (char.IsAsciiLetterUpper(letter))
        {
            int i = letter - 'A';
            return _cypher[i];
        }

        return letter;
    }

    public override IEnumerable<(char letter, char match)> GetMatches()
    {
        for (int i = 0; i < 26; i++)
            if (_cypher[i] != '-')
                yield return ((char)('A' + i), _cypher[i]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Remove(char letter)
    {
        if (char.IsAsciiLetterUpper(letter))
        {
            int i = letter - 'A';
            char match = _cypher[i];
            if (match != '-')
            {
                _cypher[i] = '-';
                i = match - 'A';
                _usedLetters[i] = false;
                _solveCount--;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(char letter)
    {
        if (char.IsAsciiLetterUpper(letter))
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool UsedContains(char letter) => char.IsAsciiLetterUpper(letter) && _usedLetters[letter - 'A'];

    public override void Clear()
    {
        Array.Fill(_cypher, '-');
        Array.Clear(_usedLetters);
        _solveCount = 0;
        base.Clear();
    }

    public override DecoderRing Clone()
    {
        return new DecoderRingArray()
        {
            _cypher = this._cypher.ToArray(),
            Hints = this.Hints.Count == 0 ? [] : this.Hints.ToHashSet(),
            _usedLetters = this._usedLetters.ToArray(),
            _solveCount = this._solveCount,
        };
    }

    public override void Overwrite(DecoderRing other)
    {
        if (other is DecoderRingArray otherArray)
        {
            Array.Copy(otherArray._cypher, _cypher, _cypher.Length);
            Array.Copy(otherArray._usedLetters, _usedLetters, _usedLetters.Length);
            _solveCount = otherArray._solveCount;
            Hints = other.Hints.Count == 0 ? [] : other.Hints.ToHashSet();
        }
        else
        {
            base.Overwrite(other);
        }
    }
    
    // overriding this for performance, it should mirror the base class's logic
    public override bool Matches(string encrypted, string candidate)
    {
        for (int i = 0; i < encrypted.Length; i++)
        {
            char letter = encrypted[i];
            char candidateMatch = candidate[i];
            if (char.IsAsciiLetterUpper(letter))
            {
                char ringMatch = _cypher[letter - 'A'];
                if (ringMatch != '-')
                {
                    if (ringMatch != candidateMatch) return false;
                }
                else if (_usedLetters[candidateMatch - 'A'])
                {
                    return false;
                }
            }
        }
        return true;
    }
}