using System.Runtime.CompilerServices;

namespace Cryptoquip.Services;

public sealed class DecoderRingArray : IDecoderRing
{
    public static IDecoderRing Create() => new DecoderRingArray();

    private DecoderRingArray() { }
    
    private char[] _cypher = Enumerable.Range(0, 26).Select(static _ => '-').ToArray();
    private bool[] _usedLetters = new bool[26];
    private int _solveCount;

    public int SolveCount => _solveCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Put(char letter, char match)
    {
        uint l = (uint)(letter - 'A');
        uint m = (uint)(match - 'A');
        if (l >= 26u || m >= 26u) return false;
        
        if (_cypher[l] != '-') return false; // is letter already mapped?
        _cypher[l] = match;
        _usedLetters[m] = true;

        _solveCount++;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char Get(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u ? _cypher[i] : letter;
    }

    public IEnumerable<(char letter, char match)> GetMatches()
    {
        for (int i = 0; i < 26; i++)
            if (_cypher[i] != '-')
                yield return ((char)('A' + i), _cypher[i]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(char letter)
    {
        uint i = (uint)(letter - 'A');
        if (i < 26u)
        {
            char match = _cypher[i];
            if (match != '-')
            {
                _cypher[i] = '-';
                uint m = (uint)(match - 'A');
                if (m < 26u)
                {
                    _usedLetters[m] = false;
                }
                _solveCount--;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u && _cypher[i] != '-';
    }

    public IEnumerable<char> GetUsedLetters()
    {
        for (int i = 0; i < _usedLetters.Length; i++)
        {
            if (_usedLetters[i])
                yield return (char)('A' + i);
        }
    }
    
    public IEnumerable<char> GetUnusedLetters()
    {
        for (int i = 0; i < _usedLetters.Length; i++)
        {
            if (!_usedLetters[i])
                yield return (char)('A' + i);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool UsedContains(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u && _usedLetters[i];
    }

    public void Clear()
    {
        Array.Fill(_cypher, '-');
        Array.Clear(_usedLetters);
        _solveCount = 0;
    }

    public IDecoderRing Clone()
    {
        return new DecoderRingArray()
        {
            _cypher = this._cypher.ToArray(),
            _usedLetters = this._usedLetters.ToArray(),
            _solveCount = this._solveCount,
        };
    }

    public void Overwrite(IDecoderRing other)
    {
        if (other is DecoderRingArray otherArray)
        {
            Array.Copy(otherArray._cypher, _cypher, _cypher.Length);
            Array.Copy(otherArray._usedLetters, _usedLetters, _usedLetters.Length);
            _solveCount = otherArray._solveCount;
        }
        else
        {
            Clear();
            foreach (var (letter, match) in other.GetMatches())
            {
                Put(letter, match);
            }
        }
    }
    
    public bool Matches(ReadOnlySpan<char> encrypted, ReadOnlySpan<char> candidate)
    {
        for (int i = 0; i < encrypted.Length; i++)
        {
            char letter = encrypted[i];
            char candidateMatch = candidate[i];
            uint l = (uint)(letter - 'A');
            if (l < 26u)
            {
                char ringMatch = _cypher[l];
                if (ringMatch != '-')
                {
                    if (ringMatch != candidateMatch) return false;
                }
                else
                {
                    uint c = (uint)(candidateMatch - 'A');
                    if (c < 26u && _usedLetters[c])
                    {
                        return false;
                    }
                }
            }
            else if (letter != candidateMatch) return false;
        }
        return true;
    }
}