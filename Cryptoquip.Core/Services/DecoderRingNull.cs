﻿namespace Cryptoquip.Services;

public class DecoderRingNull : DecoderRingAbstract
{
    public override int SolveCount => 0;
    public override void Put(char letter, char match) { }
    public override char Get(char letter) => '-';
    public override bool Matches(string encrypted, string candidate) => true;
    public override void LoadHints(ReadOnlyMemory<char> hints) { }
    public override string Decode(ReadOnlyMemory<char> message) => new('-', message.Length);
    public override void Remove(char letter) { }
    public override bool Contains(char letter) => false;
    public override bool UsedContains(char letter) => false;
    public override void Clear() { }
    public override IEnumerable<char> GetUsedLetters() => [];
    public override IEnumerable<char> GetUnusedLetters() => [];
    public override bool WasSetFromHint(char letter) => false;
    public override DecoderRingAbstract Clone() => new DecoderRingNull();
}