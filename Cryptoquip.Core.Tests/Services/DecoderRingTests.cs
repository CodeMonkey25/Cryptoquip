using Cryptoquip.Services;

namespace Cryptoquip.Tests.Services;

public class DecoderRingTests
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static TheoryData<Type> RingTypes =>
    [
        typeof(DecoderRingArray),
        typeof(DecoderRingBitmask),
        typeof(DecoderRingDictionary),
    ];

    public static TheoryData<Type, Type> RingTypePairs
    {
        get
        {
            TheoryData<Type, Type> pairs = [];
            foreach (Type target in (Type[])[typeof(DecoderRingArray), typeof(DecoderRingBitmask), typeof(DecoderRingDictionary)])
            foreach (Type source in (Type[])[typeof(DecoderRingArray), typeof(DecoderRingBitmask), typeof(DecoderRingDictionary)])
                pairs.Add(target, source);
            return pairs;
        }
    }

    private static DecoderRing Create(Type type) => (DecoderRing)Activator.CreateInstance(type)!;

    private static DecoderRing Create(Type type, string letters, string matches)
    {
        DecoderRing ring = Create(type);
        for (int i = 0; i < letters.Length; i++) ring.Put(letters[i], matches[i]);
        return ring;
    }

    private static List<(char letter, char match)> MatchList(DecoderRing ring) => ring.GetMatches().ToList();

    private static string UsedLetters(DecoderRing ring) => string.Concat(ring.GetUsedLetters());

    #region New ring

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void NewRing_IsEmpty(Type type)
    {
        DecoderRing ring = Create(type);

        Assert.Equal(0, ring.SolveCount);
        Assert.Empty(ring.GetMatches());
        Assert.Empty(ring.GetUsedLetters());
        Assert.Equal(Alphabet, string.Concat(ring.GetUnusedLetters()));
        Assert.All(Alphabet, c =>
        {
            Assert.Equal('-', ring.Get(c));
            Assert.False(ring.Contains(c));
            Assert.False(ring.UsedContains(c));
            Assert.False(ring.WasSetFromHint(c));
        });
    }

    #endregion

    #region Put and Get

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Put_NewLetter_MapsItAndReturnsTrue(Type type)
    {
        DecoderRing ring = Create(type);

        Assert.True(ring.Put('X', 'T'));

        Assert.Equal('T', ring.Get('X'));
        Assert.True(ring.Contains('X'));
        Assert.True(ring.UsedContains('T'));
        Assert.False(ring.Contains('T'));
        Assert.False(ring.UsedContains('X'));
        Assert.Equal(1, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Put_AlreadyMappedLetter_KeepsOriginalAndReturnsFalse(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");

        Assert.False(ring.Put('X', 'Q'));

        Assert.Equal('T', ring.Get('X'));
        Assert.False(ring.UsedContains('Q'));
        Assert.Equal(1, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Put_LetterToItself_IsAllowed(Type type)
    {
        DecoderRing ring = Create(type);

        Assert.True(ring.Put('A', 'A'));
        Assert.Equal('A', ring.Get('A'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Put_BoundaryLetters_AreMapped(Type type)
    {
        DecoderRing ring = Create(type, "AZ", "ZA");

        Assert.Equal('Z', ring.Get('A'));
        Assert.Equal('A', ring.Get('Z'));
        Assert.Equal(2, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Put_AllLetters_SolvesEverything(Type type)
    {
        DecoderRing ring = Create(type, Alphabet, "ZYXWVUTSRQPONMLKJIHGFEDCBA");

        Assert.Equal(26, ring.SolveCount);
        Assert.Equal(Alphabet, UsedLetters(ring));
        Assert.Empty(ring.GetUnusedLetters());
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Put_InvalidCharacters_ReturnsFalseAndChangesNothing(Type type)
    {
        DecoderRing ring = Create(type);

        Assert.False(ring.Put('\'', 'A'));
        Assert.False(ring.Put('A', '\''));
        Assert.False(ring.Put('a', 'B'));
        Assert.False(ring.Put('A', 'b'));
        Assert.False(ring.Put('@', '['));

        Assert.Equal(0, ring.SolveCount);
        Assert.Empty(ring.GetMatches());
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Get_NonLetter_ReturnsItUnchanged(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");

        Assert.Equal('\'', ring.Get('\''));
        Assert.Equal(' ', ring.Get(' '));
        Assert.Equal('1', ring.Get('1'));
        Assert.Equal('x', ring.Get('x'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Contains_NonLetter_ReturnsFalse(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");

        Assert.False(ring.Contains('\''));
        Assert.False(ring.Contains('x'));
        Assert.False(ring.UsedContains('t'));
    }

    #endregion

    #region Put spans

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void PutSpans_MapsEachPairAndReturnsCount(Type type)
    {
        DecoderRing ring = Create(type);

        int added = ring.Put("XYZ", "THE");

        Assert.Equal(3, added);
        Assert.Equal("THE", ring.Decode("XYZ".AsMemory()));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void PutSpans_SkipsAlreadyMappedAndRepeatedLetters(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");

        int added = ring.Put("XYYZ", "TOOK");

        Assert.Equal(2, added); // X was already mapped, the second Y is a repeat
        Assert.Equal("TOOK", ring.Decode("XYYZ".AsMemory()));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void PutSpans_SkipsNonLetters(Type type)
    {
        DecoderRing ring = Create(type);

        int added = ring.Put("XYZ'W", "DON'T");

        Assert.Equal(4, added);
        Assert.Equal("DON'T", ring.Decode("XYZ'W".AsMemory()));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void PutSpans_TracksOnlyNewlyAddedLetters(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");
        char[] addedLetters = new char[5];

        int added = ring.Put("XYYZ'", "TOOK'", addedLetters);

        Assert.Equal("YZ", new string(addedLetters, 0, added));
    }

    #endregion

    #region Remove

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Remove_MappedLetter_UnmapsItAndFreesMatch(Type type)
    {
        DecoderRing ring = Create(type, "XY", "TH");

        ring.Remove('X');

        Assert.Equal('-', ring.Get('X'));
        Assert.False(ring.Contains('X'));
        Assert.False(ring.UsedContains('T'));
        Assert.Equal('H', ring.Get('Y'));
        Assert.Equal(1, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Remove_ThenPut_RemapsLetter(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");

        ring.Remove('X');

        Assert.True(ring.Put('X', 'Q'));
        Assert.Equal('Q', ring.Get('X'));
        Assert.True(ring.Put('Y', 'T'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Remove_UnmappedOrInvalidLetter_ChangesNothing(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");

        ring.Remove('Y');
        ring.Remove('\'');
        ring.Remove('x');

        Assert.Equal(1, ring.SolveCount);
        Assert.Equal('T', ring.Get('X'));
        Assert.True(ring.UsedContains('T'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Remove_Twice_OnlyDecrementsOnce(Type type)
    {
        DecoderRing ring = Create(type, "XY", "TH");

        ring.Remove('X');
        ring.Remove('X');

        Assert.Equal(1, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void RemoveSpan_RemovesEachLetter(Type type)
    {
        DecoderRing ring = Create(type, "XYZ", "THE");

        ring.Remove("XZ".ToCharArray());

        Assert.Equal("-H-", ring.Decode("XYZ".AsMemory()));
        Assert.Equal("H", UsedLetters(ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void PutSpansThenRemoveAddedLetters_RestoresPreviousState(Type type)
    {
        // the solver's backtracking pattern
        DecoderRing ring = Create(type, "X", "T");
        char[] addedLetters = new char[4];

        int added = ring.Put("XYZW", "THEN", addedLetters);
        ring.Remove(addedLetters.AsSpan(0, added));

        Assert.Equal([('X', 'T')], MatchList(ring));
        Assert.Equal("T", UsedLetters(ring));
        Assert.Equal(1, ring.SolveCount);
    }

    #endregion

    #region GetMatches, GetUsedLetters and GetUnusedLetters

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_ReturnsAllMappingsInLetterOrder(Type type)
    {
        DecoderRing ring = Create(type, "ZXY", "EHT");

        Assert.Equal([('X', 'H'), ('Y', 'T'), ('Z', 'E')], ring.GetMatches());
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetUsedLetters_ReturnsMatchedPlainLettersInAlphabeticalOrder(Type type)
    {
        DecoderRing ring = Create(type, "ZXY", "EHT");

        Assert.Equal("EHT", UsedLetters(ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatchesAndGetUsedLetters_OrderIsIndependentOfInsertionOrder(Type type)
    {
        DecoderRing forward = Create(type, "QRSTU", "VWXYZ");
        DecoderRing backward = Create(type, "UTSRQ", "ZYXWV");

        Assert.Equal(forward.GetMatches(), backward.GetMatches());
        Assert.Equal(UsedLetters(forward), UsedLetters(backward));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetUnusedLetters_ReturnsComplementInAlphabeticalOrder(Type type)
    {
        DecoderRing ring = Create(type, "ZXYW", "EHTA");

        Assert.Equal("BCDFGIJKLMNOPQRSUVWXYZ", string.Concat(ring.GetUnusedLetters()));
    }

    #endregion

    #region Matches

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Matches_EmptyRing_AllowsAnyCandidate(Type type)
    {
        DecoderRing ring = Create(type);

        Assert.True(ring.Matches("XYZ", "THE"));
        Assert.True(ring.Matches("XYZ", "XYZ"));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Matches_MappedLetter_RequiresSameMatch(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");

        Assert.True(ring.Matches("XYZ", "THE"));
        Assert.False(ring.Matches("XYZ", "CAT"));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Matches_UnmappedLetter_CannotUseAlreadyUsedMatch(Type type)
    {
        DecoderRing ring = Create(type, "Q", "C"); // C is taken by Q

        Assert.True(ring.Matches("XYZ", "DOG"));
        Assert.False(ring.Matches("XYZ", "CAT"));
        Assert.False(ring.Matches("XYZ", "ARC"));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Matches_FullyMappedWord_RequiresExactDecoding(Type type)
    {
        DecoderRing ring = Create(type, "XYZ", "THE");

        Assert.True(ring.Matches("ZYX", "EHT"));
        Assert.False(ring.Matches("ZYX", "THE"));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Matches_PunctuationInSamePosition_IsAllowed(Type type)
    {
        DecoderRing ring = Create(type, "W", "T");

        Assert.True(ring.Matches("XYZ'W", "DON'T"));
        Assert.False(ring.Matches("XYZ'W", "DON'S"));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Matches_EmptyWord_ReturnsTrue(Type type)
    {
        Assert.True(Create(type, "X", "T").Matches("", ""));
    }

    #endregion

    #region Hints

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void AddHint_MarksLetter(Type type)
    {
        DecoderRing ring = Create(type);

        ring.AddHint('X');

        Assert.True(ring.WasSetFromHint('X'));
        Assert.False(ring.WasSetFromHint('Y'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void AddHint_IgnoresNonLetters(Type type)
    {
        DecoderRing ring = Create(type);

        ring.AddHint('\'');
        ring.AddHint('x');

        Assert.False(ring.WasSetFromHint('\''));
        Assert.False(ring.WasSetFromHint('x'));
        Assert.All(Alphabet, c => Assert.False(ring.WasSetFromHint(c)));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void LoadHints_SingleHint_MapsAndMarksLetters(Type type)
    {
        DecoderRing ring = Create(type);

        ring.LoadHints("XYZ=THE".AsMemory());

        Assert.Equal("THE", ring.Decode("XYZ".AsMemory()));
        Assert.All("XYZ", c => Assert.True(ring.WasSetFromHint(c)));
        Assert.Equal(3, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void LoadHints_MultipleHintsWithWhitespace_LoadsAll(Type type)
    {
        DecoderRing ring = Create(type);

        ring.LoadHints(" X = T ,Y=H,  Z =E ".AsMemory());

        Assert.Equal("THE", ring.Decode("XYZ".AsMemory()));
        Assert.All("XYZ", c => Assert.True(ring.WasSetFromHint(c)));
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("XYZ=")]
    [InlineData("=THE")]
    [InlineData("XY=THE")]
    [InlineData("XYZ=TH")]
    [InlineData("X=T=Q")]
    [InlineData("")]
    [InlineData(",,")]
    public void LoadHints_MalformedHint_IsIgnored(string hints)
    {
        foreach (Type type in (Type[])[typeof(DecoderRingArray), typeof(DecoderRingBitmask), typeof(DecoderRingDictionary)])
        {
            DecoderRing ring = Create(type);

            ring.LoadHints(hints.AsMemory());

            Assert.Equal(0, ring.SolveCount);
            Assert.All(Alphabet, c => Assert.False(ring.WasSetFromHint(c)));
        }
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void LoadHints_MalformedHintAmongValid_LoadsOnlyValid(Type type)
    {
        DecoderRing ring = Create(type);

        ring.LoadHints("XY=THE, Q=R, AB".AsMemory());

        Assert.Equal([('Q', 'R')], MatchList(ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void LoadHints_PunctuationInHint_SkipsIt(Type type)
    {
        DecoderRing ring = Create(type);

        ring.LoadHints("XYZ'W=DON'T".AsMemory());

        Assert.Equal("DON'T", ring.Decode("XYZ'W".AsMemory()));
        Assert.Equal(4, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void LoadHints_AlreadyMappedLetter_IsNotOverwrittenOrMarked(Type type)
    {
        DecoderRing ring = Create(type, "X", "Q");

        ring.LoadHints("XY=TH".AsMemory());

        Assert.Equal('Q', ring.Get('X'));
        Assert.False(ring.WasSetFromHint('X'));
        Assert.Equal('H', ring.Get('Y'));
        Assert.True(ring.WasSetFromHint('Y'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void RemoveHintedLetter_KeepsHintFlag(Type type)
    {
        DecoderRing ring = Create(type);
        ring.LoadHints("X=T".AsMemory());

        ring.Remove('X');

        Assert.False(ring.Contains('X'));
        Assert.True(ring.WasSetFromHint('X'));
    }

    #endregion

    #region Decode

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Decode_MapsKnownLettersAndDashesUnknown(Type type)
    {
        DecoderRing ring = Create(type, "XZ", "TE");

        Assert.Equal("T-E", ring.Decode("XYZ".AsMemory()));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Decode_PreservesNonLetters(Type type)
    {
        DecoderRing ring = Create(type, "XYZWQ", "DONTG");

        Assert.Equal("DON'T GO, \"DON\"!", ring.Decode("XYZ'W QY, \"XYZ\"!".AsMemory()));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Decode_Empty_ReturnsEmpty(Type type)
    {
        Assert.Equal("", Create(type).Decode(ReadOnlyMemory<char>.Empty));
    }

    #endregion

    #region Clear

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Clear_RemovesMappingsAndHints(Type type)
    {
        DecoderRing ring = Create(type);
        ring.LoadHints("XYZ=THE".AsMemory());
        ring.Put('Q', 'R');

        ring.Clear();

        Assert.Equal(0, ring.SolveCount);
        Assert.Empty(ring.GetMatches());
        Assert.Empty(ring.GetUsedLetters());
        Assert.Equal(Alphabet, string.Concat(ring.GetUnusedLetters()));
        Assert.All(Alphabet, c =>
        {
            Assert.Equal('-', ring.Get(c));
            Assert.False(ring.WasSetFromHint(c));
        });
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Clear_CanBeReusedAfterwards(Type type)
    {
        DecoderRing ring = Create(type, "X", "T");
        ring.Clear();

        Assert.True(ring.Put('X', 'Q'));
        Assert.True(ring.Put('Y', 'T'));
        Assert.Equal(2, ring.SolveCount);
    }

    #endregion

    #region Clone

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Clone_CopiesMappingsAndHints(Type type)
    {
        DecoderRing ring = Create(type, "Q", "R");
        ring.LoadHints("XYZ=THE".AsMemory());

        DecoderRing clone = ring.Clone();

        Assert.IsType(type, clone);
        Assert.NotSame(ring, clone);
        Assert.Equal(MatchList(ring), MatchList(clone));
        Assert.Equal(UsedLetters(ring), UsedLetters(clone));
        Assert.Equal(4, clone.SolveCount);
        Assert.All("XYZ", c => Assert.True(clone.WasSetFromHint(c)));
        Assert.False(clone.WasSetFromHint('Q'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Clone_IsIndependentOfOriginal(Type type)
    {
        DecoderRing ring = Create(type, "XY", "TH");
        DecoderRing clone = ring.Clone();

        clone.Put('Z', 'E');
        clone.Remove('X');
        ring.Put('Q', 'R');

        Assert.Equal([('Q', 'R'), ('X', 'T'), ('Y', 'H')], MatchList(ring));
        Assert.Equal([('Y', 'H'), ('Z', 'E')], MatchList(clone));
        Assert.True(ring.UsedContains('T'));
        Assert.False(clone.UsedContains('T'));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Clone_ClearingCloneLeavesOriginalHints(Type type)
    {
        DecoderRing ring = Create(type);
        ring.LoadHints("X=T".AsMemory());

        ring.Clone().Clear();

        Assert.True(ring.WasSetFromHint('X'));
        Assert.Equal('T', ring.Get('X'));
    }

    #endregion

    #region Overwrite

    [Theory]
    [MemberData(nameof(RingTypePairs))]
    public void Overwrite_CopiesMappingsAndHintsFromAnyImplementation(Type targetType, Type sourceType)
    {
        DecoderRing source = Create(sourceType, "Q", "R");
        source.LoadHints("XYZ=THE".AsMemory());
        DecoderRing target = Create(targetType, "AB", "CD");
        target.AddHint('A');

        target.Overwrite(source);

        Assert.Equal(MatchList(source), MatchList(target));
        Assert.Equal(UsedLetters(source), UsedLetters(target));
        Assert.Equal(source.SolveCount, target.SolveCount);
        Assert.Equal('-', target.Get('A'));
        Assert.False(target.UsedContains('C'));
        Assert.False(target.WasSetFromHint('A'));
        Assert.All("XYZ", c => Assert.True(target.WasSetFromHint(c)));
    }

    [Theory]
    [MemberData(nameof(RingTypePairs))]
    public void Overwrite_TargetIsIndependentOfSource(Type targetType, Type sourceType)
    {
        DecoderRing source = Create(sourceType, "XY", "TH");
        DecoderRing target = Create(targetType);
        target.Overwrite(source);

        target.Put('Z', 'E');
        source.Remove('X');

        Assert.Equal([('X', 'T'), ('Y', 'H'), ('Z', 'E')], MatchList(target));
        Assert.Equal([('Y', 'H')], MatchList(source));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Overwrite_WithEmptyRing_ClearsTarget(Type type)
    {
        DecoderRing target = Create(type, "XY", "TH");
        target.AddHint('X');

        target.Overwrite(Create(type));

        Assert.Equal(0, target.SolveCount);
        Assert.Empty(target.GetUsedLetters());
        Assert.False(target.WasSetFromHint('X'));
    }

    #endregion

    #region DecoderRingNull

    [Fact]
    public void Null_NeverStoresMappings()
    {
        DecoderRingNull ring = new();

        Assert.False(ring.Put('X', 'T'));
        Assert.Equal(0, ring.Put("XYZ", "THE"));
        Assert.Equal(0, ring.SolveCount);
        Assert.Empty(ring.GetMatches());
        Assert.Empty(ring.GetUsedLetters());
        Assert.Equal(Alphabet, string.Concat(ring.GetUnusedLetters()));
        Assert.All(Alphabet, c =>
        {
            Assert.Equal('-', ring.Get(c));
            Assert.False(ring.Contains(c));
            Assert.False(ring.UsedContains(c));
        });
    }

    [Fact]
    public void Null_GetNonLetter_ReturnsItUnchanged()
    {
        DecoderRingNull ring = new();

        Assert.Equal('\'', ring.Get('\''));
        Assert.Equal('x', ring.Get('x'));
    }

    [Fact]
    public void Null_MatchesAnyCandidate()
    {
        DecoderRingNull ring = new();

        Assert.True(ring.Matches("XYZ", "THE"));
        Assert.True(ring.Matches("XYZ'W", "DON'T"));
    }

    [Fact]
    public void Null_DecodesLettersAsDashes()
    {
        Assert.Equal("---'- --!", new DecoderRingNull().Decode("XYZ'W QY!".AsMemory()));
    }

    [Fact]
    public void Null_LoadHints_LoadsNothing()
    {
        DecoderRingNull ring = new();

        ring.LoadHints("XYZ=THE".AsMemory());

        Assert.Equal(0, ring.SolveCount);
        Assert.All("XYZ", c => Assert.False(ring.WasSetFromHint(c)));
    }

    [Fact]
    public void Null_RemoveAndClear_DoNothing()
    {
        DecoderRingNull ring = new();

        ring.Remove('X');
        ring.Remove("XY".ToCharArray());
        ring.Clear();

        Assert.Equal(0, ring.SolveCount);
    }

    [Fact]
    public void Null_Clone_ReturnsSameInstance()
    {
        DecoderRingNull ring = new();

        Assert.Same(ring, ring.Clone());
    }

    #endregion
}
