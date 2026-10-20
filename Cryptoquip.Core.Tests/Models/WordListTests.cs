using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
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
    private static readonly FrozenDictionary<string, ImmutableArray<string>> AllWords = new Dictionary<string, ImmutableArray<string>>()
    {
        ["A"] = ["A", "I"],
        ["AB"] = ["AN", "AT", "TO"],
        ["ABB"] = ["BEE", "SEE", "TEE", "TOO", "ZOO"],
        ["ABC"] = ["CAT", "DOG", "THE"],
        ["ABBA"] = ["NOON"],
        ["ABBC"] = ["BOOK", "LOOK", "TOOK"],
        ["ABC'D"] = ["CAN'T", "DON'T"],
        [MaxLengthWord] = [MaxLengthWord],
    }.ToFrozenDictionary();

    public static TheoryData<Type> RingTypes =>
    [
        typeof(DecoderRingArray),
        typeof(DecoderRingBitmask),
        typeof(DecoderRingDictionary),
    ];

    private static IDecoderRing CreateRing(Type ringType)
    {
        return (IDecoderRing)ringType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
    }

    [Fact]
    public void Constructor_NoPatterns_LoadsAllWordsGroupedByPattern()
    {
        WordList wordList = new();

        Assert.Equal(AllWords.Keys.Order(StringComparer.Ordinal), wordList.Words.Keys.Order(StringComparer.Ordinal));
        foreach ((string pattern, ImmutableArray<string> expectedWords) in AllWords)
        {
            Assert.Equal<string>(expectedWords, wordList.Words[pattern]);
        }
    }

    [Fact]
    public void Constructor_NoPatterns_KeysAreThePatternsOfTheirWords()
    {
        WordList wordList = new();

        foreach ((string pattern, ImmutableArray<string> words) in wordList.Words)
        {
            Assert.All(words, word => Assert.Equal(pattern, Word.MakePattern(word)));
        }
    }

    [Fact]
    public void Constructor_NoPatterns_SortsWordsOrdinally()
    {
        WordList wordList = new();

        Assert.All(wordList.Words.Values, words => Assert.Equal<string>(words.Order(StringComparer.Ordinal), words));
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

        Assert.Equal<string>([MaxLengthWord], wordList.Words[Word.MakePattern(MaxLengthWord)]);
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
        Assert.Equal<string>(AllWords["AB"], wordList.Words["AB"]);
        Assert.Equal<string>(AllWords["ABB"], wordList.Words["ABB"]);
    }

    [Fact]
    public void Constructor_WithPatterns_SupportsPunctuationInPatterns()
    {
        WordList wordList = new(["ABC'D"]);

        Assert.Equivalent((string[])["ABC'D"], wordList.Words.Keys);
        Assert.Equal<string>(["CAN'T", "DON'T"], wordList.Words["ABC'D"]);
    }

    [Fact]
    public void Constructor_WithPatterns_OmitsPatternsWithNoDictionaryWords()
    {
        WordList wordList = new(["ABC", "AAA", "ABCDE"]);

        Assert.Equivalent((string[])["ABC"], wordList.Words.Keys);
    }

    [Fact]
    public void Constructor_WithPatternLongerThanMaxWordLength_IncludesThatWord()
    {
        string pattern = Word.MakePattern(OverMaxLengthWord);
        WordList wordList = new([pattern]);

        Assert.Equal<string>([OverMaxLengthWord], wordList.Words[pattern]);
    }

    [Theory]
    [InlineData("XYZ", "ABC")]
    [InlineData("XYY", "ABB")]
    [InlineData("XYYX", "ABBA")]
    [InlineData("XYZ'W", "ABC'D")]
    public void GetMatches_NullRing_ReturnsAllWordsForPattern(string encrypted, string pattern)
    {
        WordList wordList = new();

        Assert.Equal<string>(wordList.Words[pattern], wordList.GetMatches(new Word(encrypted), DecoderRingNull.Create()));
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("XYZWV")]
    [InlineData("XY'Z")]
    public void GetMatches_PatternNotInWords_ReturnsEmpty(string encrypted)
    {
        WordList wordList = new();

        Assert.DoesNotContain(Word.MakePattern(encrypted), wordList.Words.Keys);
        Assert.Empty(wordList.GetMatches(new Word(encrypted), DecoderRingNull.Create()));
    }

    [Fact]
    public void GetMatches_ReturnsNewListWithoutModifyingWords()
    {
        WordList wordList = new();

        List<string> matches = wordList.GetMatches(new Word("XYZ"), DecoderRingNull.Create());
        matches.Clear();

        Assert.Equal<string>(AllWords["ABC"], wordList.Words["ABC"]);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_RingWithMappedLetter_ReturnsOnlyConsistentWords(Type ringType)
    {
        WordList wordList = new();
        IDecoderRing ring = CreateRing(ringType);
        ring.Put('X', 'T');

        Assert.Equal(["THE"], wordList.GetMatches(new Word("XYZ"), ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_RingWithUsedLetter_ExcludesWordsUsingItForUnmappedLetters(Type ringType)
    {
        WordList wordList = new();
        IDecoderRing ring = CreateRing(ringType);
        ring.Put('Q', 'C'); // 'C' is now taken, so no unmapped letter of XYZ can decode to it

        Assert.Equal(["DOG", "THE"], wordList.GetMatches(new Word("XYZ"), ring));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void GetMatches_RingWithMultipleMappings_ReturnsWordsSatisfyingAll(Type ringType)
    {
        WordList wordList = new();
        IDecoderRing ring = CreateRing(ringType);
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
        IDecoderRing ring = CreateRing(ringType);
        ring.Put('X', 'Q');

        Assert.Empty(wordList.GetMatches(new Word("XYZ"), ring));
    }
}
