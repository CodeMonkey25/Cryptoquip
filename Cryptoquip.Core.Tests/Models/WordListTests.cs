using Cryptoquip.Models;
using Cryptoquip.Services;

namespace Cryptoquip.Tests.Models;

/// <remarks>
/// These tests run against the small dictionary.txt in this project, which is copied to the output directory
/// in place of the full dictionary. It is intentionally unsorted and contains a blank line, words with
/// punctuation, and words exactly at and just over <c>WordList.MaxWordLength</c> (50).
/// </remarks>
public class WordListTests
{
    private const string MaxLengthWord = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWX";
    private const string OverMaxLengthWord = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXY";

    // every word in dictionary.txt within MaxWordLength, grouped by pattern and sorted ordinally
    private static readonly Dictionary<string, List<string>> AllWords = new()
    {
        ["A"] = ["A", "I"],
        ["AB"] = ["AN", "AT", "TO"],
        ["ABB"] = ["BEE", "SEE", "TEE", "TOO", "ZOO"],
        ["ABC"] = ["CAT", "DOG", "THE"],
        ["ABBA"] = ["NOON"],
        ["ABBC"] = ["BOOK", "LOOK", "TOOK"],
        ["ABC'D"] = ["CAN'T", "DON'T"],
        [MaxLengthWord] = [MaxLengthWord],
    };

    public static TheoryData<Type> RingTypes =>
    [
        typeof(DecoderRingArray),
        typeof(DecoderRingBitmask),
        typeof(DecoderRingDictionary),
    ];

    private static DecoderRing CreateRing(Type ringType) => (DecoderRing)Activator.CreateInstance(ringType)!;

    [Fact]
    public void Constructor_NoPatterns_LoadsAllWordsGroupedByPattern()
    {
        WordList wordList = new();

        Assert.Equal(AllWords, wordList.Words);
    }

    [Fact]
    public void Constructor_NoPatterns_KeysAreThePatternsOfTheirWords()
    {
        WordList wordList = new();

        foreach ((string pattern, List<string> words) in wordList.Words)
        {
            Assert.All(words, word => Assert.Equal(pattern, Word.MakePattern(word)));
        }
    }

    [Fact]
    public void Constructor_NoPatterns_SortsWordsOrdinally()
    {
        WordList wordList = new();

        Assert.All(wordList.Words.Values, words => Assert.Equal(words.Order(StringComparer.Ordinal), words));
    }

    [Fact]
    public void Constructor_NoPatterns_SkipsBlankLines()
    {
        WordList wordList = new();

        Assert.DoesNotContain("", wordList.Words.Keys);
    }

    [Fact]
    public void Constructor_NoPatterns_IncludesWordsUpToMaxWordLength()
    {
        WordList wordList = new();

        Assert.Equal([MaxLengthWord], wordList.Words[Word.MakePattern(MaxLengthWord)]);
    }

    [Fact]
    public void Constructor_NoPatterns_ExcludesWordsLongerThanMaxWordLength()
    {
        WordList wordList = new();

        Assert.DoesNotContain(Word.MakePattern(OverMaxLengthWord), wordList.Words.Keys);
    }

    [Fact]
    public void Constructor_EmptyPatterns_LoadsNoWords()
    {
        WordList wordList = new([]);

        Assert.Empty(wordList.Words);
    }

    [Fact]
    public void Constructor_WithPatterns_LoadsOnlyRequestedPatterns()
    {
        WordList wordList = new(["ABB", "AB"]);

        Assert.Equal(["AB", "ABB"], wordList.Words.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(AllWords["AB"], wordList.Words["AB"]);
        Assert.Equal(AllWords["ABB"], wordList.Words["ABB"]);
    }

    [Fact]
    public void Constructor_WithPatterns_SupportsPunctuationInPatterns()
    {
        WordList wordList = new(["ABC'D"]);

        Assert.Equal(["ABC'D"], wordList.Words.Keys);
        Assert.Equal(["CAN'T", "DON'T"], wordList.Words["ABC'D"]);
    }

    [Fact]
    public void Constructor_WithPatterns_OmitsPatternsWithNoDictionaryWords()
    {
        WordList wordList = new(["ABC", "AAA", "ABCDE"]);

        Assert.Equal(["ABC"], wordList.Words.Keys);
    }

    [Fact]
    public void Constructor_WithPatternLongerThanMaxWordLength_IncludesThatWord()
    {
        string pattern = Word.MakePattern(OverMaxLengthWord);
        WordList wordList = new([pattern]);

        Assert.Equal([OverMaxLengthWord], wordList.Words[pattern]);
    }

    [Theory]
    [InlineData("XYZ", "ABC")]
    [InlineData("XYY", "ABB")]
    [InlineData("XYYX", "ABBA")]
    [InlineData("XYZ'W", "ABC'D")]
    public void GetMatches_NullRing_ReturnsAllWordsForPattern(string encrypted, string pattern)
    {
        WordList wordList = new();

        Assert.Equal(wordList.Words[pattern], wordList.GetMatches(new Word(encrypted), new DecoderRingNull()));
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("XYZWV")]
    [InlineData("XY'Z")]
    public void GetMatches_PatternNotInWords_ReturnsEmpty(string encrypted)
    {
        WordList wordList = new();

        Assert.DoesNotContain(Word.MakePattern(encrypted), wordList.Words.Keys);
        Assert.Empty(wordList.GetMatches(new Word(encrypted), new DecoderRingNull()));
    }

    [Fact]
    public void GetMatches_ReturnsNewListWithoutModifyingWords()
    {
        WordList wordList = new();

        List<string> matches = wordList.GetMatches(new Word("XYZ"), new DecoderRingNull());
        matches.Clear();

        Assert.NotSame(wordList.Words["ABC"], matches);
        Assert.Equal(AllWords["ABC"], wordList.Words["ABC"]);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_RingWithMappedLetter_ReturnsOnlyConsistentWords(Type ringType)
    {
        WordList wordList = new();
        DecoderRing ring = CreateRing(ringType);
        ring.Put('X', 'T');

        Assert.Equal(["THE"], wordList.GetMatches(new Word("XYZ"), ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_RingWithUsedLetter_ExcludesWordsUsingItForUnmappedLetters(Type ringType)
    {
        WordList wordList = new();
        DecoderRing ring = CreateRing(ringType);
        ring.Put('Q', 'C'); // 'C' is now taken, so no unmapped letter of XYZ can decode to it

        Assert.Equal(["DOG", "THE"], wordList.GetMatches(new Word("XYZ"), ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_RingWithMultipleMappings_ReturnsWordsSatisfyingAll(Type ringType)
    {
        WordList wordList = new();
        DecoderRing ring = CreateRing(ringType);
        ring.Put('Y', 'O');
        ring.Put('Z', 'K');

        Assert.Equal(["BOOK", "LOOK", "TOOK"], wordList.GetMatches(new Word("XYYZ"), ring));

        ring.Put('X', 'L');

        Assert.Equal(["LOOK"], wordList.GetMatches(new Word("XYYZ"), ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_RingConflictingWithAllCandidates_ReturnsEmpty(Type ringType)
    {
        WordList wordList = new();
        DecoderRing ring = CreateRing(ringType);
        ring.Put('X', 'Q');

        Assert.Empty(wordList.GetMatches(new Word("XYZ"), ring));
    }
}
