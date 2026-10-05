using Cryptoquip.Models;

namespace Cryptoquip.Tests.Models;

public class WordTests
{
    private static uint Mask(string letters) => letters.Aggregate(0u, (mask, c) => mask | 1u << (c - 'A'));

    private static Word WithMatches(string text, params string[] matches) => new(text) { Matches = [..matches] };

    #region Constructor

    [Fact]
    public void Constructor_SetsTextAndPattern()
    {
        Word word = new("XYX");

        Assert.Equal("XYX", word.Text);
        Assert.Equal("ABA", word.Pattern);
    }

    [Fact]
    public void Constructor_StartsWithEmptyMatchesPerInstance()
    {
        Word first = new("XYZ");
        Word second = new("XYZ");

        Assert.Empty(first.Matches);
        Assert.NotSame(first.Matches, second.Matches);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("A", "A")]
    [InlineData("XYZ", "XYZ")]
    [InlineData("XYXYX", "XY")]
    [InlineData("X'Y", "XY")]
    [InlineData("X Y-Z1", "XYZ")]
    [InlineData("ZYXWVUTSRQPONMLKJIHGFEDCBA", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public void Constructor_LetterMaskHasBitForEachDistinctUppercaseLetter(string text, string letters)
    {
        Assert.Equal(Mask(letters), new Word(text).LetterMask);
    }

    [Fact]
    public void Constructor_LetterMaskIgnoresLowercaseLetters()
    {
        Assert.Equal(0u, new Word("xyz").LetterMask);
    }

    [Theory]
    [InlineData("XYZ", true)]
    [InlineData("A", true)]
    [InlineData("DON'T", true)]
    [InlineData("X1", true)]
    [InlineData("", false)]
    [InlineData("'", false)]
    [InlineData("123", false)]
    [InlineData("xyz", false)]
    [InlineData("XY ZW", false)]
    [InlineData("XY\tZW", false)]
    public void Constructor_IsSolvableRequiresALetterAndNoWhitespace(string text, bool expected)
    {
        Assert.Equal(expected, new Word(text).IsSolvable);
    }

    #endregion

    #region MakePattern

    [Theory]
    [InlineData("", "")]
    [InlineData("X", "A")]
    [InlineData("XYZ", "ABC")]
    [InlineData("THE", "ABC")]
    [InlineData("XYX", "ABA")]
    [InlineData("QQQ", "AAA")]
    [InlineData("HELLO", "ABCCD")]
    [InlineData("NOON", "ABBA")]
    [InlineData("ZYXWVUTSRQPONMLKJIHGFEDCBA", "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public void MakePattern_AssignsLettersInOrderOfFirstAppearance(string text, string expected)
    {
        Assert.Equal(expected, Word.MakePattern(text));
    }

    [Theory]
    [InlineData("DON'T", "ABC'D")]
    [InlineData("X Y X", "A B A")]
    [InlineData("X-1-X", "A-1-A")]
    [InlineData("123", "123")]
    [InlineData("xyz", "xyz")]
    [InlineData("XxX", "AxA")]
    public void MakePattern_PreservesCharactersThatAreNotUppercaseLetters(string text, string expected)
    {
        Assert.Equal(expected, Word.MakePattern(text));
    }

    [Fact]
    public void MakePattern_TextLongerThanMaxWordLength_StillMakesPattern()
    {
        string text = string.Concat(Enumerable.Repeat("XYZ", 30)); // 90 chars, past the stackalloc limit
        string expected = string.Concat(Enumerable.Repeat("ABC", 30));

        Assert.Equal(expected, Word.MakePattern(text));
    }

    [Fact]
    public void MakePattern_WithBuffers_ReturnsPatternOfTextLengthOnly()
    {
        char[] patternBuffer = new char[10];

        string pattern = Word.MakePattern("XYX", patternBuffer, new char[26], new int[26]);

        Assert.Equal("ABA", pattern);
    }

    [Fact]
    public void MakePattern_WithBuffers_RestoresLetterBufferToZero()
    {
        char[] letterBuffer = new char[26];

        Word.MakePattern("HELLO", new char[5], letterBuffer, new int[26]);

        Assert.All(letterBuffer, c => Assert.Equal('\0', c));
    }

    [Fact]
    public void MakePattern_WithBuffers_CanReuseBuffersAcrossCalls()
    {
        char[] patternBuffer = new char[10];
        char[] letterBuffer = new char[26];
        int[] touchedBuffer = new int[26];

        Assert.Equal("ABCCD", Word.MakePattern("HELLO", patternBuffer, letterBuffer, touchedBuffer));
        Assert.Equal("ABA", Word.MakePattern("LOL", patternBuffer, letterBuffer, touchedBuffer));
        Assert.Equal("ABC", Word.MakePattern("OHL", patternBuffer, letterBuffer, touchedBuffer));
    }

    [Fact]
    public void MakePattern_WithBuffers_PatternBufferTooShort_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Word.MakePattern("XYZ", new char[2], new char[26], new int[26]));
    }

    [Fact]
    public void MakePattern_WithBuffers_LetterBufferTooShort_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Word.MakePattern("XYZ", new char[3], new char[25], new int[26]));
    }

    [Fact]
    public void MakePattern_WithBuffers_TouchedBufferTooShort_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Word.MakePattern("XYZ", new char[3], new char[26], new int[25]));
    }

    #endregion

    #region WritePattern

    [Fact]
    public void WritePattern_WritesPatternWithoutTouchingRestOfBuffer()
    {
        char[] patternBuffer = "**********".ToCharArray();

        Word.WritePattern("DON'T", patternBuffer, new char[26], new int[26]);

        Assert.Equal("ABC'D*****", new string(patternBuffer));
    }

    [Fact]
    public void WritePattern_RestoresLetterBufferToZero()
    {
        char[] letterBuffer = new char[26];

        Word.WritePattern("ZYXWVUTSRQPONMLKJIHGFEDCBA", new char[26], letterBuffer, new int[26]);

        Assert.All(letterBuffer, c => Assert.Equal('\0', c));
    }

    #endregion

    #region GetMatchRequirements

    [Fact]
    public void GetMatchRequirements_AllowsEachLetterAnyPlainLetterSeenInMatches()
    {
        // X may be A or C, Y may be B or D
        MatchRequirements requirements = WithMatches("XY", "AB", "CD").GetMatchRequirements();

        Assert.True(requirements.Matches("XY", "AB"));
        Assert.True(requirements.Matches("XY", "CD"));
        Assert.True(requirements.Matches("XY", "AD"));
        Assert.False(requirements.Matches("XY", "BA"));
        Assert.False(requirements.Matches("XY", "AQ"));
    }

    [Fact]
    public void GetMatchRequirements_AppliesToOtherWordsSharingLetters()
    {
        MatchRequirements requirements = WithMatches("XY", "AB", "CD").GetMatchRequirements();

        Assert.True(requirements.Matches("YZ", "BQ")); // Z is unconstrained
        Assert.True(requirements.Matches("YZ", "DQ"));
        Assert.False(requirements.Matches("YZ", "QQ"));
    }

    [Fact]
    public void GetMatchRequirements_NoMatches_ConstrainsNothing()
    {
        MatchRequirements requirements = new Word("XY").GetMatchRequirements();

        Assert.True(requirements.Matches("XY", "QR"));
    }

    [Fact]
    public void GetMatchRequirements_IsSnapshotOfCurrentMatches()
    {
        Word word = WithMatches("XY", "AB");
        MatchRequirements requirements = word.GetMatchRequirements();

        word.Matches.Add("CD");

        Assert.False(requirements.Matches("XY", "CD"));
    }

    #endregion

    #region EnsureMatchRequirements

    [Fact]
    public void EnsureMatchRequirements_RemovesUnmatchedWordsPreservingOrder()
    {
        MatchRequirements requirements = WithMatches("XY", "AB", "CD").GetMatchRequirements(); // Y may be B or D
        Word word = WithMatches("YZ", "BA", "QA", "DC", "AA", "BQ");

        int removed = word.EnsureMatchRequirements(requirements);

        Assert.Equal(2, removed);
        Assert.Equal(["BA", "DC", "BQ"], word.Matches);
    }

    [Fact]
    public void EnsureMatchRequirements_AllMatchesSatisfy_RemovesNothing()
    {
        MatchRequirements requirements = WithMatches("XY", "AB", "CD").GetMatchRequirements();
        Word word = WithMatches("YZ", "BA", "DC");
        List<string> matches = word.Matches;

        int removed = word.EnsureMatchRequirements(requirements);

        Assert.Equal(0, removed);
        Assert.Same(matches, word.Matches);
        Assert.Equal(["BA", "DC"], word.Matches);
    }

    [Fact]
    public void EnsureMatchRequirements_NoMatchesSatisfy_RemovesAll()
    {
        MatchRequirements requirements = WithMatches("XY", "AB").GetMatchRequirements();
        Word word = WithMatches("YZ", "QA", "RA", "SA");

        int removed = word.EnsureMatchRequirements(requirements);

        Assert.Equal(3, removed);
        Assert.Empty(word.Matches);
    }

    [Fact]
    public void EnsureMatchRequirements_NoSharedLetters_RemovesNothing()
    {
        MatchRequirements requirements = WithMatches("XY", "AB").GetMatchRequirements();
        Word word = WithMatches("QR", "CD", "EF");

        Assert.Equal(0, word.EnsureMatchRequirements(requirements));
        Assert.Equal(["CD", "EF"], word.Matches);
    }

    [Fact]
    public void EnsureMatchRequirements_NoMatches_ReturnsZero()
    {
        MatchRequirements requirements = WithMatches("XY", "AB").GetMatchRequirements();

        Assert.Equal(0, new Word("YZ").EnsureMatchRequirements(requirements));
    }

    #endregion

    #region CompareTo

    [Fact]
    public void CompareTo_Self_ReturnsZero()
    {
        Word word = WithMatches("XYZ", "THE");

        Assert.Equal(0, word.CompareTo(word));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        Assert.Equal(1, new Word("XYZ").CompareTo(null));
    }

    [Fact]
    public void CompareTo_SameTextDifferentMatches_ReturnsZero()
    {
        Word first = WithMatches("XYZ", "THE");
        Word second = WithMatches("XYZ", "CAT", "DOG");

        Assert.Equal(0, first.CompareTo(second));
        Assert.Equal(0, second.CompareTo(first));
    }

    [Fact]
    public void CompareTo_FewerMatchesSortsFirst_RegardlessOfLength()
    {
        Word fewer = WithMatches("XY", "AN");
        Word more = WithMatches("XYZWV", "HOUSE", "MOUSE");

        Assert.True(fewer.CompareTo(more) < 0);
        Assert.True(more.CompareTo(fewer) > 0);
    }

    [Fact]
    public void CompareTo_SameMatchCount_LongerTextSortsFirst()
    {
        Word longer = WithMatches("XYZW", "BOOK");
        Word shorter = WithMatches("ABC", "THE");

        Assert.True(longer.CompareTo(shorter) < 0);
        Assert.True(shorter.CompareTo(longer) > 0);
    }

    [Fact]
    public void CompareTo_SameMatchCountAndLength_OrdersTextOrdinally()
    {
        Word first = WithMatches("ABC", "THE");
        Word second = WithMatches("ABD", "CAT");

        Assert.True(first.CompareTo(second) < 0);
        Assert.True(second.CompareTo(first) > 0);
    }

    [Fact]
    public void CompareTo_SortsByMatchCountThenLengthDescendingThenText()
    {
        Word twoMatches = WithMatches("Q", "A", "I");
        Word oneMatchShortB = WithMatches("XB", "AN");
        Word oneMatchShortA = WithMatches("XA", "TO");
        Word oneMatchLong = WithMatches("XYZ", "THE");
        Word noMatches = new("XYZWV");

        List<Word> words = [twoMatches, oneMatchShortB, oneMatchShortA, oneMatchLong, noMatches];
        words.Sort();

        Assert.Equal([noMatches, oneMatchLong, oneMatchShortA, oneMatchShortB, twoMatches], words);
    }

    #endregion
}
