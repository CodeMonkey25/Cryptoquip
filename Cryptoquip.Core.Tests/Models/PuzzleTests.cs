using Cryptoquip.Models;
using Cryptoquip.Services;

namespace Cryptoquip.Tests.Models;

public class PuzzleTests
{
    private static List<string> AllWords(Puzzle puzzle) => puzzle.GetAllWords().Select(static w => w.ToString()).ToList();

    #region Parse

    [Fact]
    public void Parse_UppercasesAndTrimsOriginalText()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("  Xyz abc  ");

        Assert.Equal("XYZ ABC", puzzle.OriginalText);
        Assert.Equal("XYZ ABC", puzzle.Text.ToString());
    }

    [Fact]
    public void Parse_NoHint_LeavesRingEmpty()
    {
        (_, DecoderRing ring) = Puzzle.Parse("XYZ ABC");

        Assert.Equal(0, ring.SolveCount);
        Assert.All("ABCXYZ", c => Assert.False(ring.WasSetFromHint(c)));
    }

    [Fact]
    public void Parse_ClearsExistingRingState()
    {
        DecoderRing ring = new DecoderRingArray();
        ring.Put('Q', 'R');
        ring.AddHint('Q');

        _ = Puzzle.Parse("XYZ", ring);

        Assert.False(ring.Contains('Q'));
        Assert.False(ring.WasSetFromHint('Q'));
    }

    [Fact]
    public void Parse_WithHint_RemovesHintFromTextButKeepsItInOriginalText()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("XYZ ABC <HINT>: X=T");

        Assert.Equal("XYZ ABC <HINT>: X=T", puzzle.OriginalText);
        Assert.Equal("XYZ ABC", puzzle.Text.ToString().Trim());
        Assert.Equal(["XYZ", "ABC"], AllWords(puzzle));
    }

    [Fact]
    public void Parse_WithHint_LoadsHintIntoRing()
    {
        (_, DecoderRing ring) = Puzzle.Parse("XYZ ABC <HINT>: X=T");

        Assert.Equal('T', ring.Get('X'));
        Assert.True(ring.WasSetFromHint('X'));
        Assert.Equal(1, ring.SolveCount);
    }

    [Fact]
    public void Parse_WithMultipleHints_LoadsAllIntoRing()
    {
        (_, DecoderRing ring) = Puzzle.Parse("XYZ ABC <HINT>: XYZ=THE, A=C");

        Assert.Equal('T', ring.Get('X'));
        Assert.Equal('H', ring.Get('Y'));
        Assert.Equal('E', ring.Get('Z'));
        Assert.Equal('C', ring.Get('A'));
        Assert.Equal('-', ring.Get('B'));
        Assert.All("XYZA", c => Assert.True(ring.WasSetFromHint(c)));
        Assert.False(ring.WasSetFromHint('B'));
    }

    [Fact]
    public void Parse_WithLowercaseHint_IsCaseInsensitive()
    {
        (Puzzle puzzle, DecoderRing ring) = Puzzle.Parse("xyz abc <hint>: x=t");

        Assert.Equal(["XYZ", "ABC"], AllWords(puzzle));
        Assert.Equal('T', ring.Get('X'));
    }

    [Fact]
    public void Parse_WithEmptyHint_LoadsNothing()
    {
        (Puzzle puzzle, DecoderRing ring) = Puzzle.Parse("XYZ <HINT>: ");

        Assert.Equal(["XYZ"], AllWords(puzzle));
        Assert.Equal(0, ring.SolveCount);
    }

    [Theory]
    [InlineData("  XYZ <HINT>: X=T")]
    [InlineData("XYZ <HINT>: X=T  ")]
    [InlineData("  XYZ <HINT>: X=T  ")]
    [InlineData("\r\nXYZ <HINT>: X=T\r\n")]
    public void Parse_WithHintAndSurroundingWhitespace_LoadsHint(string text)
    {
        (Puzzle puzzle, DecoderRing ring) = Puzzle.Parse(text);

        Assert.Equal(["XYZ"], AllWords(puzzle));
        Assert.Equal('T', ring.Get('X'));
        Assert.True(ring.WasSetFromHint('X'));
    }

    #endregion

    #region GetAllWords

    [Fact]
    public void GetAllWords_SplitsOnSpacesIgnoringRepeats()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("XYZ   ABC Q");

        Assert.Equal(["XYZ", "ABC", "Q"], AllWords(puzzle));
    }

    [Fact]
    public void GetAllWords_KeepsPunctuationAndDuplicates()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("XYZ, \"ABC\" XYZ. 123 X-Y !");

        Assert.Equal(["XYZ,", "\"ABC\"", "XYZ.", "123", "X-Y", "!"], AllWords(puzzle));
    }

    [Fact]
    public void GetAllWords_EmptyText_ReturnsNothing()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("   ");

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
        (Puzzle puzzle, _) = Puzzle.Parse(text);

        Assert.Equal([expected], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_KeepsApostrophes()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("XYZ'W 'QR'");

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
        (Puzzle puzzle, _) = Puzzle.Parse(text);

        Assert.Empty(puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_RemovesDuplicatesKeepingFirstOccurrenceOrder()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("ABC XYZ ABC, XYZ. QR \"ABC\"");
        Assert.Equal(["ABC", "XYZ", "QR"], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_ExcludesHint()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("XYZ ABC. <HINT>: X=T");

        Assert.Equal(["XYZ", "ABC"], puzzle.GetFilteredAndDistinctWords());
    }

    [Fact]
    public void GetFilteredAndDistinctWords_MixedPuzzle()
    {
        (Puzzle puzzle, _) = Puzzle.Parse("Qmf XYZ'W, \"Qmf\" 42 R-S xyz!");
        Assert.Equal(["QMF", "XYZ'W", "XYZ"], puzzle.GetFilteredAndDistinctWords());
    }

    #endregion
}
