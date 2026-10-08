using Cryptoquip.Models;

namespace Cryptoquip.Tests.Models;

public class PuzzleTests
{
    private static List<string> AllWords(Puzzle puzzle) => puzzle.GetAllWords().Select(static w => w.ToString()).ToList();

    #region Parse

    [Fact]
    public void Parse_UppercasesAndTrimsOriginalText()
    {
        Puzzle puzzle = Puzzle.Parse("  Xyz abc  ");

        Assert.Equal("XYZ ABC", puzzle.OriginalText);
        Assert.Equal("XYZ ABC", puzzle.Text.ToString());
    }

    [Fact]
    public void Parse_NoHint_HasNoHints()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ ABC");

        Assert.Empty(puzzle.Hints);
    }

    [Fact]
    public void Parse_WithHint_RemovesHintFromTextButKeepsItInOriginalText()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ ABC <HINT>: X=T");

        Assert.Equal("XYZ ABC <HINT>: X=T", puzzle.OriginalText);
        Assert.Equal("XYZ ABC", puzzle.Text.ToString().Trim());
        Assert.Equal(["XYZ", "ABC"], AllWords(puzzle));
    }

    [Fact]
    public void Parse_WithHint_ParsesHint()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ ABC <HINT>: X=T");

        Assert.Equal(new Dictionary<char, char> { ['X'] = 'T' }, puzzle.Hints);
    }

    [Fact]
    public void Parse_WithMultipleHints_ParsesAll()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ ABC <HINT>: XYZ=THE, A=C");

        Assert.Equal(new Dictionary<char, char> { ['X'] = 'T', ['Y'] = 'H', ['Z'] = 'E', ['A'] = 'C' }, puzzle.Hints);
    }

    [Fact]
    public void Parse_WithMultipleHintsAndWhitespace_ParsesAll()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ <HINT>:  X = T ,Y=H,  Z =E ");

        Assert.Equal(new Dictionary<char, char> { ['X'] = 'T', ['Y'] = 'H', ['Z'] = 'E' }, puzzle.Hints);
    }

    [Fact]
    public void Parse_WithLowercaseHint_IsCaseInsensitive()
    {
        Puzzle puzzle = Puzzle.Parse("xyz abc <hint>: x=t");

        Assert.Equal(["XYZ", "ABC"], AllWords(puzzle));
        Assert.Equal(new Dictionary<char, char> { ['X'] = 'T' }, puzzle.Hints);
    }

    [Fact]
    public void Parse_WithEmptyHint_HasNoHints()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ <HINT>: ");

        Assert.Equal(["XYZ"], AllWords(puzzle));
        Assert.Empty(puzzle.Hints);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("XYZ=")]
    [InlineData("=THE")]
    [InlineData("XY=THE")]
    [InlineData("XYZ=TH")]
    [InlineData("X=T=Q")]
    [InlineData(",,")]
    public void Parse_WithMalformedHint_IgnoresIt(string hint)
    {
        Puzzle puzzle = Puzzle.Parse($"XYZ <HINT>: {hint}");

        Assert.Equal(["XYZ"], AllWords(puzzle));
        Assert.Empty(puzzle.Hints);
    }

    [Fact]
    public void Parse_WithMalformedHintAmongValid_ParsesOnlyValid()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ <HINT>: XY=THE, Q=R, AB");

        Assert.Equal(new Dictionary<char, char> { ['Q'] = 'R' }, puzzle.Hints);
    }

    [Fact]
    public void Parse_WithPunctuationInHint_IgnoresPunctuationAndParsesLetters()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ'W <HINT>: XYZ'W=DON'T");

        Assert.Equal(new Dictionary<char, char> { ['X'] = 'D', ['Y'] = 'O', ['Z'] = 'N', ['W'] = 'T' }, puzzle.Hints);
    }
    
    [Fact]
    public void Parse_WithRepeatedLetterInHints_LastMappingWins()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ <HINT>: X=T, X=Q");

        Assert.Equal(new Dictionary<char, char> { ['X'] = 'Q' }, puzzle.Hints);
    }

    [Theory]
    [InlineData("  XYZ <HINT>: X=T")]
    [InlineData("XYZ <HINT>: X=T  ")]
    [InlineData("  XYZ <HINT>: X=T  ")]
    [InlineData("\r\nXYZ <HINT>: X=T\r\n")]
    public void Parse_WithHintAndSurroundingWhitespace_ParsesHint(string text)
    {
        Puzzle puzzle = Puzzle.Parse(text);

        Assert.Equal(["XYZ"], AllWords(puzzle));
        Assert.Equal(new Dictionary<char, char> { ['X'] = 'T' }, puzzle.Hints);
    }

    #endregion

    #region GetAllWords

    [Fact]
    public void GetAllWords_SplitsOnSpacesIgnoringRepeats()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ   ABC Q");

        Assert.Equal(["XYZ", "ABC", "Q"], AllWords(puzzle));
    }

    [Fact]
    public void GetAllWords_KeepsPunctuationAndDuplicates()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ, \"ABC\" XYZ. 123 X-Y !");

        Assert.Equal(["XYZ,", "\"ABC\"", "XYZ.", "123", "X-Y", "!"], AllWords(puzzle));
    }

    [Fact]
    public void GetAllWords_EmptyText_ReturnsNothing()
    {
        Puzzle puzzle = Puzzle.Parse("   ");

        Assert.Empty(puzzle.GetAllWords());
    }

    #endregion

    #region GetFilteredAndDistinctWords

    [Theory]
    [InlineData("XYZ.", "XYZ")]
    [InlineData("XYZ,", "XYZ")]
    [InlineData("XYZ!", "XYZ")]
    [InlineData("XYZ?", "XYZ")]
    [InlineData("XYZ;", "XYZ")]
    [InlineData("XYZ:", "XYZ")]
    [InlineData("\"XYZ\"", "XYZ")]
    [InlineData("\"XYZ?!\"", "XYZ")]
    public void GetFilteredAndDistinctWords_TrimsSurroundingPunctuation(string text, string expected)
    {
        Puzzle puzzle = Puzzle.Parse(text);

        Assert.Equal([expected], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_KeepsApostrophes()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ'W 'QR'");

        Assert.Equal(["XYZ'W", "'QR'"], puzzle.GetFilteredAndDistinctWords());
    }

    [Theory]
    [InlineData("X-Y")]
    [InlineData("X.Y")]
    [InlineData("123")]
    [InlineData("X1")]
    [InlineData("(XYZ)")]
    [InlineData("!?.")]
    public void GetFilteredAndDistinctWords_ExcludesWordsWithOtherCharacters(string text)
    {
        Puzzle puzzle = Puzzle.Parse(text);

        Assert.Empty(puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_RemovesDuplicatesKeepingFirstOccurrenceOrder()
    {
        Puzzle puzzle = Puzzle.Parse("ABC XYZ ABC, XYZ. QR \"ABC\"");
        Assert.Equal(["ABC", "XYZ", "QR"], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_ExcludesHint()
    {
        Puzzle puzzle = Puzzle.Parse("XYZ ABC. <HINT>: X=T");

        Assert.Equal(["XYZ", "ABC"], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_MixedPuzzle()
    {
        Puzzle puzzle = Puzzle.Parse("Qmf XYZ'W, \"Qmf\" 42 R-S xyz!");
        Assert.Equal(["QMF", "XYZ'W", "XYZ"], puzzle.GetFilteredAndDistinctWords());
    }

    #endregion
}
