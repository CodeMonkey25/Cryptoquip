using Cryptoquip.Models;

namespace Cryptoquip.Tests.Models;

public class MatchRequirementsTests
{
    public static TheoryData<Type> RequirementTypes =>
    [
        typeof(MatchRequirementsArray),
        typeof(MatchRequirementsBitmask),
        typeof(MatchRequirementsDictionary),
    ];

    private static MatchRequirements Create(Type type, string text, params string[] matches)
    {
        MatchRequirements requirements = (MatchRequirements)Activator.CreateInstance(type)!;
        requirements.Rebuild(text, [..matches]);
        return requirements;
    }

    #region Build

    [Fact]
    public void Build_ReturnsBitmaskImplementation()
    {
        Assert.IsType<MatchRequirementsBitmask>(MatchRequirements.Build());
    }

    [Fact]
    public void Build_ReturnsNewInstanceEachCall()
    {
        Assert.NotSame(MatchRequirements.Build(), MatchRequirements.Build());
    }

    [Fact]
    public void Build_StartsUnconstrained()
    {
        MatchRequirements requirements = MatchRequirements.Build();

        Assert.True(requirements.Matches("XYZ", "QRS"));
    }

    [Fact]
    public void Build_WithMatches_RegistersThem()
    {
        MatchRequirements requirements = MatchRequirements.Build("XY", ["AB", "CD"]);

        Assert.IsType<MatchRequirementsBitmask>(requirements);
        Assert.True(requirements.Matches("XY", "AB"));
        Assert.False(requirements.Matches("XY", "QR"));
    }

    #endregion

    #region Matches (all implementations)

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_NoRegisteredMatches_AllowsAnything(Type type)
    {
        MatchRequirements requirements = Create(type, "XY");

        Assert.True(requirements.Matches("XY", "QR"));
        Assert.True(requirements.Matches("ABC", "DEF"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_RegisteredMatch_Matches(Type type)
    {
        MatchRequirements requirements = Create(type, "XYZ", "THE");

        Assert.True(requirements.Matches("XYZ", "THE"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_ConstrainedLetterWithOtherPlainLetter_DoesNotMatch(Type type)
    {
        MatchRequirements requirements = Create(type, "XYZ", "THE");

        Assert.False(requirements.Matches("XYZ", "CAT"));
        Assert.False(requirements.Matches("XYZ", "THY")); // only the last letter differs
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_AllowsAnyPlainLetterSeenPerCipherLetterIndependently(Type type)
    {
        // X may be A or C, Y may be B or D
        MatchRequirements requirements = Create(type, "XY", "AB", "CD");

        Assert.True(requirements.Matches("XY", "AB"));
        Assert.True(requirements.Matches("XY", "CD"));
        Assert.True(requirements.Matches("XY", "AD"));
        Assert.True(requirements.Matches("XY", "CB"));
        Assert.False(requirements.Matches("XY", "BA"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_RepeatedCipherLetter_AccumulatesAllPositions(Type type)
    {
        // X appears twice, so it may be N (from both positions of NOON) and nothing else
        MatchRequirements requirements = Create(type, "XYYX", "NOON");

        Assert.True(requirements.Matches("XQ", "NZ"));
        Assert.False(requirements.Matches("XQ", "OZ"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_AppliesToDifferentTextSharingLetters(Type type)
    {
        MatchRequirements requirements = Create(type, "XY", "AB", "CD");

        Assert.True(requirements.Matches("YZ", "BQ"));
        Assert.True(requirements.Matches("ZY", "QD"));
        Assert.False(requirements.Matches("YZ", "AQ"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_UnconstrainedLettersAllowAnything(Type type)
    {
        MatchRequirements requirements = Create(type, "XY", "AB");

        Assert.True(requirements.Matches("QRS", "ABC"));
        Assert.True(requirements.Matches("QRS", "XYZ"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_ConstrainedLetterWithNonLetterCandidate_DoesNotMatch(Type type)
    {
        MatchRequirements requirements = Create(type, "XY", "AB");

        Assert.False(requirements.Matches("XY", "A'"));
        Assert.False(requirements.Matches("XY", "-B"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_TextWithPunctuation_ConstrainsLetters(Type type)
    {
        MatchRequirements requirements = Create(type, "XYZ'W", "DON'T", "CAN'T");

        Assert.True(requirements.Matches("XYZ'W", "DAN'T"));
        Assert.False(requirements.Matches("XYZ'W", "WON'T"));
        Assert.False(requirements.Matches("XYZ'W", "DON'S"));
    }

    #endregion

    #region Rebuild and Clear (all implementations)

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Rebuild_ReplacesPreviousRequirements(Type type)
    {
        MatchRequirements requirements = Create(type, "XY", "AB");

        requirements.Rebuild("XY", ["CD"]);

        Assert.True(requirements.Matches("XY", "CD"));
        Assert.False(requirements.Matches("XY", "AB"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Rebuild_WithDifferentText_DropsConstraintsOnOldLetters(Type type)
    {
        MatchRequirements requirements = Create(type, "XY", "AB");

        requirements.Rebuild("QR", ["CD"]);

        Assert.True(requirements.Matches("XY", "MN")); // X and Y are no longer constrained
        Assert.False(requirements.Matches("QR", "MN"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Rebuild_WithNoMatches_RemovesAllConstraints(Type type)
    {
        MatchRequirements requirements = Create(type, "XY", "AB");

        requirements.Rebuild("XY", []);

        Assert.True(requirements.Matches("XY", "QR"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Rebuild_DoesNotTrackLaterChangesToMatchesList(Type type)
    {
        MatchRequirements requirements = (MatchRequirements)Activator.CreateInstance(type)!;
        List<string> matches = ["AB"];
        requirements.Rebuild("XY", matches);

        matches.Add("CD");

        Assert.False(requirements.Matches("XY", "CD"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Clear_RemovesAllConstraints(Type type)
    {
        MatchRequirements requirements = Create(type, "XYZ", "THE", "CAT");

        requirements.Clear();

        Assert.True(requirements.Matches("XYZ", "DOG"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Clear_CanBeReusedAfterwards(Type type)
    {
        MatchRequirements requirements = Create(type, "XYZ", "THE");
        requirements.Clear();

        requirements.Rebuild("XYZ", ["DOG"]);

        Assert.True(requirements.Matches("XYZ", "DOG"));
        Assert.False(requirements.Matches("XYZ", "THE"));
    }

    #endregion

    #region Non-letter characters (all implementations)

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_IgnoresNonLetterCipherCharacters(Type type)
    {
        MatchRequirements requirements = Create(type, "X'", "A'");

        Assert.True(requirements.Matches("'Y", "QB")); // apostrophe is not constrained
        Assert.True(requirements.Matches("'Y", "'B"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Matches_IgnoresLowercaseLetters(Type type)
    {
        MatchRequirements requirements = Create(type, "xy", "ab");

        Assert.True(requirements.Matches("xy", "QR"));
        Assert.True(requirements.Matches("xy", "ab"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Rebuild_IgnoresNonLetterPlainCharacters(Type type)
    {
        // X never gets a valid plain letter registered, so it stays unconstrained
        MatchRequirements requirements = Create(type, "XY", "'B");

        Assert.True(requirements.Matches("XY", "QB"));
        Assert.False(requirements.Matches("XY", "QC"));
    }

    [Theory]
    [MemberData(nameof(RequirementTypes))]
    public void Rebuild_IgnoresLowercasePlainCharacters(Type type)
    {
        MatchRequirements requirements = Create(type, "XY", "aB");

        Assert.True(requirements.Matches("XY", "QB"));
        Assert.False(requirements.Matches("XY", "QC"));
    }

    #endregion
}
