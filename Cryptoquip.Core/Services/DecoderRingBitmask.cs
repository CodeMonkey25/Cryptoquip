using System.Numerics;
using System.Runtime.CompilerServices;

namespace Cryptoquip.Services;

public sealed class DecoderRingBitmask : DecoderRing
{
    private char[] _cypher = Enumerable.Range(0, 26).Select(static _ => '-').ToArray();
    private uint _usedLetters;
    private uint _mappedLetters;
    private int _solveCount;

    public override int SolveCount => _solveCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Put(char letter, char match)
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
    public override char Get(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u ? _cypher[i] : letter;
    }

    public override IEnumerable<(char letter, char match)> GetMatches()
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
    public override void Remove(char letter)
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
    public override bool Contains(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u && (_mappedLetters & (1u << (int)i)) != 0;
    }

    public override IEnumerable<char> GetUsedLetters()
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
    public override bool UsedContains(char letter)
    {
        uint i = (uint)(letter - 'A');
        return i < 26u && (_usedLetters & (1u << (int)i)) != 0;
    }

    public override void Clear()
    {
        Array.Fill(_cypher, '-');
        _usedLetters = 0;
        _mappedLetters = 0;
        _solveCount = 0;
        base.Clear();
    }

    public override DecoderRing Clone()
    {
        return new DecoderRingBitmask()
        {
            _cypher = this._cypher.ToArray(),
            Hints = this.Hints,
            _usedLetters = this._usedLetters,
            _mappedLetters = this._mappedLetters,
            _solveCount = this._solveCount,
        };
    }

    public override void Overwrite(DecoderRing other)
    {
        if (other is DecoderRingBitmask otherBitmask)
        {
            Array.Copy(otherBitmask._cypher, _cypher, _cypher.Length);
            _usedLetters = otherBitmask._usedLetters;
            _mappedLetters = otherBitmask._mappedLetters;
            _solveCount = otherBitmask._solveCount;
            Hints = other.Hints;
        }
        else
        {
            base.Overwrite(other);
        }
    }

    // overriding this for performance, it should mirror the base class's logic
    public override bool Matches(ReadOnlySpan<char> encrypted, ReadOnlySpan<char> candidate)
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
        }
        return true;
    }
    
    // overriding this for performance, it should mirror the base class's logic
    public override IEnumerable<char> GetUnusedLetters()
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
