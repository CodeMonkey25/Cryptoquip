using Cryptoquip.Models;
using Cryptoquip.Services;

namespace Cryptoquip.Tests.Models;

public class PuzzleTests
{
    private static List<string> AllWords(Puzzle puzzle) => puzzle.GetAllWords().Select(static w => w.ToString()).ToList();

    #region Constructor

    [Fact]
    public void Constructor_UppercasesAndTrimsOriginalText()
    {
        Puzzle puzzle = new("  Xyz abc  ", new DecoderRingArray());

        Assert.Equal("XYZ ABC", puzzle.OriginalText);
        Assert.Equal("XYZ ABC", puzzle.Text.ToString());
    }

    [Fact]
    public void Constructor_NoHint_LeavesRingEmpty()
    {
        DecoderRing ring = new DecoderRingArray();

        _ = new Puzzle("XYZ ABC", ring);

        Assert.Equal(0, ring.SolveCount);
        Assert.All("ABCXYZ", c => Assert.False(ring.WasSetFromHint(c)));
    }

    [Fact]
    public void Constructor_ClearsExistingRingState()
    {
        DecoderRing ring = new DecoderRingArray();
        ring.Put('Q', 'R');
        ring.AddHint('Q');

        _ = new Puzzle("XYZ", ring);

        Assert.False(ring.Contains('Q'));
        Assert.False(ring.WasSetFromHint('Q'));
    }

    [Fact]
    public void Constructor_WithHint_RemovesHintFromTextButKeepsItInOriginalText()
    {
        Puzzle puzzle = new("XYZ ABC <HINT>: X=T", new DecoderRingArray());

        Assert.Equal("XYZ ABC <HINT>: X=T", puzzle.OriginalText);
        Assert.Equal("XYZ ABC", puzzle.Text.ToString().Trim());
        Assert.Equal(["XYZ", "ABC"], AllWords(puzzle));
    }

    [Fact]
    public void Constructor_WithHint_LoadsHintIntoRing()
    {
        DecoderRing ring = new DecoderRingArray();

        _ = new Puzzle("XYZ ABC <HINT>: X=T", ring);

        Assert.Equal('T', ring.Get('X'));
        Assert.True(ring.WasSetFromHint('X'));
        Assert.Equal(1, ring.SolveCount);
    }

    [Fact]
    public void Constructor_WithMultipleHints_LoadsAllIntoRing()
    {
        DecoderRing ring = new DecoderRingArray();

        _ = new Puzzle("XYZ ABC <HINT>: XYZ=THE, A=C", ring);

        Assert.Equal('T', ring.Get('X'));
        Assert.Equal('H', ring.Get('Y'));
        Assert.Equal('E', ring.Get('Z'));
        Assert.Equal('C', ring.Get('A'));
        Assert.Equal('-', ring.Get('B'));
        Assert.All("XYZA", c => Assert.True(ring.WasSetFromHint(c)));
        Assert.False(ring.WasSetFromHint('B'));
    }

    [Fact]
    public void Constructor_WithLowercaseHint_IsCaseInsensitive()
    {
        DecoderRing ring = new DecoderRingArray();

        Puzzle puzzle = new("xyz abc <hint>: x=t", ring);

        Assert.Equal(["XYZ", "ABC"], AllWords(puzzle));
        Assert.Equal('T', ring.Get('X'));
    }

    [Fact]
    public void Constructor_WithEmptyHint_LoadsNothing()
    {
        DecoderRing ring = new DecoderRingArray();

        Puzzle puzzle = new("XYZ <HINT>:", ring);

        Assert.Equal(["XYZ"], AllWords(puzzle));
        Assert.Equal(0, ring.SolveCount);
    }

    [Theory]
    [InlineData("  XYZ <HINT>: X=T")]
    [InlineData("XYZ <HINT>: X=T  ")]
    [InlineData("  XYZ <HINT>: X=T  ")]
    [InlineData("\r\nXYZ <HINT>: X=T\r\n")]
    public void Constructor_WithHintAndSurroundingWhitespace_LoadsHint(string text)
    {
        DecoderRing ring = new DecoderRingArray();

        Puzzle puzzle = new(text, ring);

        Assert.Equal(["XYZ"], AllWords(puzzle));
        Assert.Equal('T', ring.Get('X'));
        Assert.True(ring.WasSetFromHint('X'));
    }

    #endregion

    #region GetAllWords

    [Fact]
    public void GetAllWords_SplitsOnSpacesIgnoringRepeats()
    {
        Puzzle puzzle = new("XYZ   ABC Q", new DecoderRingArray());

        Assert.Equal(["XYZ", "ABC", "Q"], AllWords(puzzle));
    }

    [Fact]
    public void GetAllWords_KeepsPunctuationAndDuplicates()
    {
        Puzzle puzzle = new("XYZ, \"ABC\" XYZ. 123 X-Y !", new DecoderRingArray());

        Assert.Equal(["XYZ,", "\"ABC\"", "XYZ.", "123", "X-Y", "!"], AllWords(puzzle));
    }

    [Fact]
    public void GetAllWords_EmptyText_ReturnsNothing()
    {
        Puzzle puzzle = new("   ", new DecoderRingArray());

        Assert.Empty(puzzle.GetAllWords());
    }

    [Fact]
    public void GetAllWords_UsesCurrentText()
    {
        Puzzle puzzle = new("XYZ ABC", new DecoderRingArray());

        puzzle.Text = "QRS".AsMemory();

        Assert.Equal(["QRS"], AllWords(puzzle));
        Assert.Equal("XYZ ABC", puzzle.OriginalText);
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
        Puzzle puzzle = new(text, new DecoderRingArray());

        Assert.Equal([expected], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_KeepsApostrophes()
    {
        Puzzle puzzle = new("XYZ'W 'QR'", new DecoderRingArray());

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
        Puzzle puzzle = new(text, new DecoderRingArray());

        Assert.Empty(puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_RemovesDuplicatesKeepingFirstOccurrenceOrder()
    {
        Puzzle puzzle = new("ABC XYZ ABC, XYZ. QR \"ABC\"", new DecoderRingArray());

        Assert.Equal(["ABC", "XYZ", "QR"], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_ExcludesHint()
    {
        Puzzle puzzle = new("XYZ ABC. <HINT>: X=T", new DecoderRingArray());

        Assert.Equal(["XYZ", "ABC"], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_MixedPuzzle()
    {
        Puzzle puzzle = new("Qmf XYZ'W, \"Qmf\" 42 R-S xyz!", new DecoderRingArray());

        Assert.Equal(["QMF", "XYZ'W", "XYZ"], puzzle.GetFilteredAndDistinctWords());
    }

    #endregion
}
