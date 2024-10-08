using System.Collections.Generic;

namespace Cryptoquip.Utility;

public interface IDecoderRing
{
    int SolveCount { get; }
    void Put(char letter, char match);
    char Get(char letter);
    bool Matches(string encrypted, string candidate);
    void LoadHints(string hints);
    string Decode(string message);
    void Remove(char letter);
    bool Contains(char letter);
    void Clear();
    IEnumerable<char> GetUsedLetters();
    bool WasSetFromHint(char letter);
    IDecoderRing Clone();
}