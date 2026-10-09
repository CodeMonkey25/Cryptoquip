using Cryptoquip.Models;
using Cryptoquip.Services;

namespace Cryptoquip.Tests.Services;

public class ExclusionAnalysisTests
{
    private static Word WithMatches(string text, params string[] matches) => new(text) { Matches = [..matches] };

    private static int TotalMatches(Word[] words) => words.Sum(static w => w.Matches.Count);

    private static void AssertFullyConsistent(Word[] words)
    {
        for (int i = 0; i < words.Length; i++)
        {
            MatchRequirements requirements = words[i].GetMatchRequirements();
            for (int j = 0; j < words.Length; j++)
            {
                if (i == j) continue;
                Assert.All(words[j].Matches, match => Assert.True(requirements.Matches(words[j].Text, match),
                    $"{words[j].Text}={match} conflicts with {words[i].Text}"));
            }
        }
    }

    [Fact]
    public void Run_NoWords_ReturnsZero()
    {
        Assert.Equal(0, new ExclusionAnalysis().Run([]));
    }

    [Fact]
    public void Run_SingleWord_RemovesNothing()
    {
        Word word = WithMatches("XYZ", "CAT", "DOG", "THE");

        int deleted = new ExclusionAnalysis().Run([word]);

        Assert.Equal(0, deleted);
        Assert.Equal(["CAT", "DOG", "THE"], word.Matches);
    }

    [Fact]
    public void Run_NoSharedLetters_RemovesNothing()
    {
        Word first = WithMatches("XY", "AB");
        Word second = WithMatches("QR", "CD", "EF");

        int deleted = new ExclusionAnalysis().Run([first, second]);

        Assert.Equal(0, deleted);
        Assert.Equal(["AB"], first.Matches);
        Assert.Equal(["CD", "EF"], second.Matches);
    }

    [Fact]
    public void Run_AlreadyConsistent_RemovesNothing()
    {
        Word first = WithMatches("XY", "AB", "CD");
        Word second = WithMatches("YZ", "BE", "DF");

        int deleted = new ExclusionAnalysis().Run([first, second]);

        Assert.Equal(0, deleted);
        Assert.Equal(["AB", "CD"], first.Matches);
        Assert.Equal(["BE", "DF"], second.Matches);
    }

    [Fact]
    public void Run_SharedLetter_RemovesInconsistentMatches()
    {
        Word first = WithMatches("XY", "AB"); // Y must be B
        Word second = WithMatches("YZ", "BC", "DC", "BE");

        int deleted = new ExclusionAnalysis().Run([first, second]);

        Assert.Equal(1, deleted);
        Assert.Equal(["AB"], first.Matches);
        Assert.Equal(["BC", "BE"], second.Matches);
    }

    [Fact]
    public void Run_PrunesInBothDirections()
    {
        Word first = WithMatches("XY", "AB", "CD");  // Y may be B or D
        Word second = WithMatches("YZ", "BE", "FG"); // Y may be B or F

        int deleted = new ExclusionAnalysis().Run([first, second]);

        Assert.Equal(2, deleted);
        Assert.Equal(["AB"], first.Matches);
        Assert.Equal(["BE"], second.Matches);
    }

    [Fact]
    public void Run_PropagatesThroughWordsWithoutDirectlySharedLetters()
    {
        Word first = WithMatches("XY", "AB");         // Y must be B
        Word second = WithMatches("YZ", "BC", "DE");  // pruned to BC, so Z must be C
        Word third = WithMatches("ZW", "CF", "EG");   // shares nothing with first, pruned via second

        int deleted = new ExclusionAnalysis().Run([first, second, third]);

        Assert.Equal(2, deleted);
        Assert.Equal(["BC"], second.Matches);
        Assert.Equal(["CF"], third.Matches);
    }

    [Fact]
    public void Run_WordWithNoMatches_DoesNotConstrainOthers()
    {
        Word unknown = new("XY"); // e.g. a proper noun missing from the dictionary
        Word other = WithMatches("YZ", "BC", "DE");

        int deleted = new ExclusionAnalysis().Run([unknown, other]);

        Assert.Equal(0, deleted);
        Assert.Equal(["BC", "DE"], other.Matches);
    }

    [Fact]
    public void Run_CanPruneWordToNoMatches()
    {
        Word first = WithMatches("XY", "AB");
        Word second = WithMatches("YZ", "QC", "RC");
        Word third = WithMatches("ZW", "CD");

        int deleted = new ExclusionAnalysis().Run([first, second, third]);

        Assert.Equal(2, deleted);
        Assert.Empty(second.Matches);
        Assert.Equal(["CD"], third.Matches); // an emptied word imposes no constraints
    }

    [Fact]
    public void Run_KeepsRemainingMatchesInOrder()
    {
        Word first = WithMatches("XY", "AB", "AD");
        Word second = WithMatches("YZ", "DZ", "QZ", "BY", "RR", "BA");

        new ExclusionAnalysis().Run([first, second]);

        Assert.Equal(["DZ", "BY", "BA"], second.Matches);
    }

    [Fact]
    public void Run_HandlesPunctuationInWords()
    {
        Word contraction = WithMatches("XYZ'W", "CAN'T", "DON'T"); // W must be T
        Word other = WithMatches("WQ", "TO", "SO");

        int deleted = new ExclusionAnalysis().Run([contraction, other]);

        Assert.Equal(1, deleted);
        Assert.Equal(["TO"], other.Matches);
    }

    [Fact]
    public void Run_ReturnsTotalNumberOfRemovedMatches()
    {
        Word[] words =
        [
            WithMatches("XY", "AB", "CD"),
            WithMatches("YZ", "BE", "FG", "DH"),
            WithMatches("ZW", "EI", "GJ", "KL"),
            WithMatches("QR", "MN"),
        ];
        int before = TotalMatches(words);

        int deleted = new ExclusionAnalysis().Run(words);

        Assert.Equal(before - TotalMatches(words), deleted);
        Assert.True(deleted > 0);
    }

    [Fact]
    public void Run_DoesNotReorderWords()
    {
        Word first = WithMatches("XY", "AB", "CD", "EF");
        Word second = WithMatches("YZ", "BG");
        Word third = WithMatches("QR", "HI", "JK");
        Word[] words = [first, second, third];

        new ExclusionAnalysis().Run(words);

        Assert.Equal([first, second, third], words);
    }

    [Fact]
    public void Run_LeavesEveryPairOfWordsConsistent()
    {
        Word[] words =
        [
            WithMatches("XY", "AB", "CD", "EF"),
            WithMatches("YZ", "BG", "DH", "QQ"),
            WithMatches("ZW", "GI", "HJ", "KL"),
            WithMatches("WX", "IA", "JC", "LE"),
        ];

        new ExclusionAnalysis().Run(words);

        AssertFullyConsistent(words);
    }

    [Fact]
    public void Run_SecondRun_RemovesNothing()
    {
        Word[] words =
        [
            WithMatches("XY", "AB", "CD"),
            WithMatches("YZ", "BE", "FG"),
            WithMatches("ZW", "EH", "GI"),
        ];
        ExclusionAnalysis analysis = new();
        analysis.Run(words);

        Assert.Equal(0, analysis.Run(words));
    }

    [Fact]
    public void Run_WithWordListMatches_NarrowsPuzzleWords()
    {
        // XYZ is CAT/DOG/THE, ZWW is BEE/SEE/TEE/TOO/ZOO; the shared Z can only be T
        WordList wordList = new();
        Word[] words = [new("XYZ"), new("ZWW")];
        foreach (Word word in words) word.Matches = wordList.GetMatches(word, DecoderRingNull.Create());

        int deleted = new ExclusionAnalysis().Run(words);

        Assert.Equal(5, deleted);
        Assert.Equal(["CAT"], words[0].Matches);
        Assert.Equal(["TEE", "TOO"], words[1].Matches);
    }
}
