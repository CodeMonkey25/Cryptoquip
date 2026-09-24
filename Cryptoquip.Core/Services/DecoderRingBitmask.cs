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
    public override void Put(char letter, char match)
    {
        if (char.IsAsciiLetterUpper(letter))
        {
            int i = letter - 'A';
            _cypher[i] = match;
            _mappedLetters |= 1u << i;

            i = match - 'A';
            _usedLetters |= 1u << i;

            _solveCount++;
        }
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
        if (char.IsAsciiLetterUpper(letter))
        {
            int i = letter - 'A';
            char match = _cypher[i];
            if (match != '-')
            {
                _cypher[i] = '-';
                _mappedLetters &= ~(1u << i);
                i = match - 'A';
                _usedLetters &= ~(1u << i);
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
            return (_mappedLetters & (1u << i)) != 0;
        }

        return false;
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
    public override bool UsedContains(char letter) => (_usedLetters & (1u << (letter - 'A'))) != 0;

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
            Hints = this.Hints.Count == 0 ? [] : this.Hints.ToHashSet(),
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
                else if ((_usedLetters & (1u << (candidateMatch - 'A'))) != 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
