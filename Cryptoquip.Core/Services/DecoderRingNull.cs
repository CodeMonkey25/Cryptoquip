namespace Cryptoquip.Services;

public sealed class DecoderRingNull : IDecoderRing
{
    public static IDecoderRing Create() => new DecoderRingNull();

    private DecoderRingNull() { }
    
    public int SolveCount => 0;
    public bool Put(char letter, char match) => false;
    public char Get(char letter) => char.IsAsciiLetterUpper(letter) ? '-' : letter;
    public IEnumerable<(char letter, char match)> GetMatches() => [];
    public void Remove(char letter) { }
    public bool Contains(char letter) => false;
    public bool UsedContains(char letter) => false;
    public void Clear() { }
    public IEnumerable<char> GetUsedLetters() => [];
    public IDecoderRing Clone() => this;
}