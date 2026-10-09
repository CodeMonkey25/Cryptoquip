using System.Numerics;
using System.Runtime.CompilerServices;

namespace Cryptoquip.Services;

public sealed class DecoderRingBitmask : IDecoderRing
{
    public static IDecoderRing Create() => new DecoderRingBitmask();

    private DecoderRingBitmask() { }
    
    private char[] _cypher = Enumerable.Range(0, 26).Select(static _ => '-').ToArray();
    private uint _usedLetters;
    private uint _mappedLetters;
    private int _solveCount;

    public int SolveCount => _solveCount;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Put(char letter, char match)
    {
        uint l = (uint)(letter - 'A');
        uint m = (uint)(match - 'A');
        if (l >= 26u || m >= 26u) return false;
        
        uint lBit = 1u << (int)l;
        if ((_mappedLetters & lBit) != 0) return false; // is letter already mapped?
        _cypher[l] = match;
        _mappedLetters |= lBit;

        _usedLetters |= 1u << (int)m;

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
        uint mapped = _mappedLetters;
        while (mapped != 0)
        {
            int i = BitOperations.TrailingZeroCount(mapped);
            yield return ((char)('A' + i), _cypher[i]);
            mapped &= mapped - 1;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(char letter)
    {
        uint i = (uint)(letter - 'A');
        if (i < 26u)
        {
            uint lBit = 1u << (int)i;
            if ((_mappedLetters & lBit) != 0)
            {
                char match = _cypher[i];
                _cypher[i] = '-';
                _mappedLetters &= ~lBit;
                uint m = (uint)(match - 'A');
                if (m < 26u)
                {
                    _usedLetters &= ~(1u << (int)m);
                }
                _solveCount--;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u && (_mappedLetters & (1u << (int)i)) != 0;
    }

    public IEnumerable<char> GetUsedLetters()
    {
        uint used = _usedLetters;
        while (used != 0)
        {
            int i = BitOperations.TrailingZeroCount(used);
            yield return (char)('A' + i);
            used &= used - 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool UsedContains(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u && (_usedLetters & (1u << (int)i)) != 0;
    }

    public void Clear()
    {
        Array.Fill(_cypher, '-');
        _usedLetters = 0;
        _mappedLetters = 0;
        _solveCount = 0;
    }

    public IDecoderRing Clone()
    {
        return new DecoderRingBitmask()
        {
            _cypher = this._cypher.ToArray(),
            _usedLetters = this._usedLetters,
            _mappedLetters = this._mappedLetters,
            _solveCount = this._solveCount,
        };
    }

    public void Overwrite(IDecoderRing other)
    {
        if (other is DecoderRingBitmask otherBitmask)
        {
            Array.Copy(otherBitmask._cypher, _cypher, _cypher.Length);
            _usedLetters = otherBitmask._usedLetters;
            _mappedLetters = otherBitmask._mappedLetters;
            _solveCount = otherBitmask._solveCount;
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

    // overriding this for performance, it should mirror the base class's logic
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
                    if (c < 26u && (_usedLetters & (1u << (int)c)) != 0)
                    {
                        return false;
                    }
                }
            }
            else if (letter != candidateMatch) return false;
        }
        return true;
    }
    
    public IEnumerable<char> GetUnusedLetters()
    {
        uint unused = ~_usedLetters & 0x03FFFFFFu;
        while (unused != 0)
        {
            int i = BitOperations.TrailingZeroCount(unused);
            yield return (char)('A' + i);
            unused &= unused - 1;
        }
    }
}
