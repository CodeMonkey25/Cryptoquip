using System.Collections.Generic;
using System.Linq;

namespace Cryptoquip.Utility;

public class DecoderRingNull : DecoderRingAbstract
{
    public override int SolveCount => 0;
    public override void Put(char letter, char match) { }
    public override char Get(char letter) => '-';
    public override bool Matches(string encrypted, string candidate) => true;
    public override void LoadHints(string hints) { }
    public override string Decode(string message) => new('-', message.Length);
    public override void Remove(char letter) { }
    public override bool Contains(char letter) => false;
    public override bool UsedContains(char letter) => false;
    public override void Clear() { }
    public override IEnumerable<char> GetUnusedLetters() => Enumerable.Empty<char>();
    public override bool WasSetFromHint(char letter) => false;
    public override DecoderRingAbstract Clone() => new DecoderRingNull();
}