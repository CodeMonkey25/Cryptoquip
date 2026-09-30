namespace Cryptoquip.Services;

public sealed class DecoderRingNull : DecoderRing
{
    public override int SolveCount => 0;
    public override bool Put(char letter, char match) => false;
    public override char Get(char letter) => char.IsAsciiLetterUpper(letter) ? '-' : letter;
    public override IEnumerable<(char letter, char match)> GetMatches() => [];
    public override void Remove(char letter) { }
    public override bool Contains(char letter) => false;
    public override bool UsedContains(char letter) => false;
    public override IEnumerable<char> GetUsedLetters() => [];
    public override DecoderRing Clone() => this;
}