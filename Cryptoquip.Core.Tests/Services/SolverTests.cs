using System.Reflection;
using Cryptoquip.Models;
using Cryptoquip.Services;

namespace Cryptoquip.Tests.Services;

/// <remarks>
/// These tests run against the small dictionary.txt in this project. The words relevant here are
/// A, I, AN, AT, TO, BEE, SEE, TEE, TOO, ZOO, CAT, DOG, THE, NOON, BOOK, LOOK, TOOK, CAN'T, and DON'T.
/// </remarks>
public class SolverTests
{
    private const string NoSolutionMessage = "Could not find a solution. Printing the best attempt.";
    private const string NoWordListMessage = "No word list provided. Loading word list from disk.";

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

    private static (List<string> log, IDecoderRing ring) Solve(string text, IDecoderRing? ring = null, WordList? wordList = null, bool enableExclusionAnalysis = false)
    {
        Puzzle puzzle = Puzzle.Parse(text);
        ring ??= IDecoderRing.Create();
        ring.Put(puzzle.Hints);
        List<string> log = [];

        new Solver().Run(log.Add, puzzle, ring, wordList, enableExclusionAnalysis);

        return (log, ring);
    }

    #region Solving

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Run_SingleCandidateWord_SolvesIt(Type ringType)
    {
        (List<string> log, IDecoderRing ring) = Solve("XYYX", CreateRing(ringType)!);

        Assert.Equal("NOON", log[^1]);
        Assert.Equal([('X', 'N'), ('Y', 'O')], ring.GetMatches());
        Assert.DoesNotContain(NoSolutionMessage, log);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Run_SharedLetters_FindsConsistentSolution(Type ringType)
    {
        // XYZ and QRX are both CAT/DOG/THE; only THE + CAT agree on X
        (List<string> log, _) = Solve("XYZ QRX", CreateRing(ringType)!);

        Assert.Equal("THE CAT", log[^1]);
        Assert.DoesNotContain(NoSolutionMessage, log);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Run_FirstCandidatesFail_BacktracksToSolution(Type ringType)
    {
        // ABC is tried first; CAT and DOG leave no match for DEA, so the solver must back out of both
        (List<string> log, IDecoderRing ring) = Solve("ABC DEA", CreateRing(ringType)!);

        Assert.Equal("THE CAT", log[^1]);
        Assert.Equal([('A', 'T'), ('B', 'H'), ('C', 'E'), ('D', 'C'), ('E', 'A')], ring.GetMatches());
    }

    [Fact]
    public void Run_MultipleSolutions_PicksFirstInDictionaryOrder()
    {
        (List<string> log, _) = Solve("XYZ");

        Assert.Equal("CAT", log[^1]);
    }

    [Fact]
    public void Run_Hint_NarrowsSolution()
    {
        (List<string> log, IDecoderRing ring) = Solve("XYZ <HINT>: X=D");

        Assert.Equal("DOG", log[^1]);
        Assert.Equal('D', ring.Get('X'));
        Assert.Equal('O', ring.Get('Y'));
        Assert.Equal('G', ring.Get('Z'));
    }

    [Fact]
    public void Run_PunctuationInPuzzle_IsPreservedInSolution()
    {
        // YW is ?T where Y is A (CAN'T) or O (DON'T); only AT is in the dictionary
        (List<string> log, _) = Solve("XYZ'W, YW!");

        Assert.Equal("CAN'T, AT!", log[^1]);
    }

    [Fact]
    public void Run_RepeatedWords_AreSolvedOnce()
    {
        (List<string> log, _) = Solve("XYYX XYYX. XYYX!");

        Assert.Contains("Found 1 unique words to solve.", log);
        Assert.Equal("NOON NOON. NOON!", log[^1]);
    }

    [Fact]
    public void Run_WordsWithInvalidCharacters_AreIgnoredButKeptInOutput()
    {
        (List<string> log, _) = Solve("XYYX 123 X2");

        Assert.Contains("Found 1 unique words to solve.", log);
        Assert.Equal("NOON 123 N2", log[^1]);
    }

    #endregion

    #region Unsolvable

    [Fact]
    public void Run_WordWithNoMatches_IsSkippedAndLeftUndecoded()
    {
        (List<string> log, _) = Solve("QQQ XYYX");

        Assert.Contains("The word 'QQQ' is unsolvable - skipping this word", log);
        Assert.Equal("--- NOON", log[^1]);
        Assert.DoesNotContain(NoSolutionMessage, log);
    }

    [Fact]
    public void Run_WordWithNoMatches_IsStillDecodedFromOtherWords()
    {
        // XYQXY has no dictionary match, but X and Y are solved by XYYX
        (List<string> log, _) = Solve("XYQXY XYYX");

        Assert.Contains("The word 'XYQXY' is unsolvable - skipping this word", log);
        Assert.Equal("NO-NO NOON", log[^1]);
    }

    [Fact]
    public void Run_OnlyUnsolvableWords_PrintsUndecodedPuzzle()
    {
        (List<string> log, IDecoderRing ring) = Solve("QQQ");

        Assert.Equal("---", log[^1]);
        Assert.Equal(0, ring.SolveCount);
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Run_NoConsistentSolution_RestoresBestPartialAttempt(Type ringType)
    {
        // QYZ and XYZ must both end in the same two letters, which no pair of dictionary words does
        (List<string> log, IDecoderRing ring) = Solve("XYZ QYZ", CreateRing(ringType)!);

        Assert.Contains(NoSolutionMessage, log);
        Assert.Equal("-AT CAT", log[^1]);
        Assert.Equal([('Q', 'C'), ('Y', 'A'), ('Z', 'T')], ring.GetMatches());
    }

    [Fact]
    public void Run_HintConflictingWithEveryCandidate_LeavesOnlyTheHint()
    {
        (List<string> log, IDecoderRing ring) = Solve("XYZ <HINT>: X=Q");

        Assert.Contains("The word 'XYZ' is unsolvable - skipping this word", log);
        Assert.Equal("Q--", log[^1]);
        Assert.Equal([('X', 'Q')], ring.GetMatches());
    }

    [Fact]
    public void Run_EmptyPuzzle_PrintsEmptySolution()
    {
        (List<string> log, _) = Solve("");

        Assert.Contains("Found 0 unique words to solve.", log);
        Assert.Equal("", log[^1]);
    }

    #endregion

    #region Word list

    [Fact]
    public void Run_NoWordList_LoadsFromDisk()
    {
        (List<string> log, _) = Solve("XYYX");

        Assert.Contains(NoWordListMessage, log);
        Assert.Equal("NOON", log[^1]);
    }

    [Fact]
    public void Run_WordListProvided_UsesIt()
    {
        (List<string> log, _) = Solve("XYYX", wordList: new WordList());

        Assert.DoesNotContain(NoWordListMessage, log);
        Assert.Equal("NOON", log[^1]);
    }

    [Fact]
    public void Run_WordListMissingPattern_TreatsWordAsUnsolvable()
    {
        (List<string> log, _) = Solve("XYYX", wordList: new WordList(["ABC"]));

        Assert.Contains("The word 'XYYX' is unsolvable - skipping this word", log);
        Assert.Equal("----", log[^1]);
    }

    #endregion

    #region Exclusion analysis

    [Fact]
    public void Run_ExclusionAnalysisDisabled_DoesNotRunIt()
    {
        (List<string> log, _) = Solve("XYZ QRX");

        Assert.DoesNotContain("Performing exclusion analysis...", log);
        Assert.DoesNotContain(log, static m => m.StartsWith("Deleted "));
    }

    [Theory]
    [MemberData(nameof(RingTypes))]
    public void Run_ExclusionAnalysisEnabled_PrunesMatchesAndSolves(Type ringType)
    {
        (List<string> log, _) = Solve("XYZ QRX", CreateRing(ringType)!, enableExclusionAnalysis: true);

        Assert.Contains("Performing exclusion analysis...", log);
        Assert.Contains("Deleted 4 words...", log);
        Assert.Contains("\tXYZ (1)", log);
        Assert.Contains("\tQRX (1)", log);
        Assert.Equal("THE CAT", log[^1]);
    }

    [Fact]
    public void Run_ExclusionAnalysisEnabled_FindsSameSolutionAsWithout()
    {
        const string text = "ABC DEA XYYX";

        (List<string> without, _) = Solve(text);
        (List<string> with, _) = Solve(text, enableExclusionAnalysis: true);

        Assert.Equal("THE CAT NOON", without[^1]);
        Assert.Equal(without[^1], with[^1]);
    }

    #endregion

    #region Logging

    [Fact]
    public void Run_LogsPuzzleFirstAndSolutionLast()
    {
        (List<string> log, _) = Solve("xyz qrx <hint>: x=t");

        Assert.Equal("Received puzzle: XYZ QRX", log[0]);
        Assert.Equal(string.Empty, log[^2]);
        Assert.Equal("THE CAT", log[^1]);
    }

    [Fact]
    public void Run_LogsEachWordWithItsMatchCountInSortedOrder()
    {
        (List<string> log, _) = Solve("XYZ XYYX QQQ");

        int start = log.IndexOf("Loading matches...") + 1;
        int end = log.IndexOf("Word matches are ready.");

        Assert.Equal(["\tQQQ (0)", "\tXYYX (1)", "\tXYZ (3)"], log[start..end]);
    }

    #endregion
}
