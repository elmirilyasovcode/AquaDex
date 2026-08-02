using AquaDex.Core.Entities;
using AquaDex.Core.Enums;
using Xunit;

namespace AquaDex.Tests;



public class CatchLogPrivacyLogicTests
{
    private static (decimal? lat, decimal? lon) ApplyPrivacyRule(CatchLog log, bool forceShowLocation)
    {
        var showLocation = forceShowLocation || log.ShareExactLocation;
        return (showLocation ? log.Latitude : null, showLocation ? log.Longitude : null);
    }

    private static CatchLog MakeCatchLog(bool shareExactLocation) => new()
    {
        Latitude = 40.4093m,
        Longitude = 49.8671m,
        ShareExactLocation = shareExactLocation,
        CaughtAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public void PublicView_ShareExactLocationFalse_CoordinatesAreHidden()
    {
        var log = MakeCatchLog(shareExactLocation: false);

        var (lat, lon) = ApplyPrivacyRule(log, forceShowLocation: false);

        Assert.Null(lat);
        Assert.Null(lon);
    }

    [Fact]
    public void PublicView_ShareExactLocationTrue_CoordinatesAreVisible()
    {
        var log = MakeCatchLog(shareExactLocation: true);

        var (lat, lon) = ApplyPrivacyRule(log, forceShowLocation: false);

        Assert.Equal(40.4093m, lat);
        Assert.Equal(49.8671m, lon);
    }

    [Fact]
    public void OwnerView_ShareExactLocationFalse_CoordinatesStillVisibleToOwner()
    {
        var log = MakeCatchLog(shareExactLocation: false);

        var (lat, lon) = ApplyPrivacyRule(log, forceShowLocation: true);

        Assert.Equal(40.4093m, lat);
        Assert.Equal(49.8671m, lon);
    }

    [Fact]
    public void NoCoordinatesRecorded_RemainsNullRegardlessOfPrivacySetting()
    {
        var log = new CatchLog
        {
            Latitude = null,
            Longitude = null,
            ShareExactLocation = true,
            CaughtAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var (lat, lon) = ApplyPrivacyRule(log, forceShowLocation: false);

        Assert.Null(lat);
        Assert.Null(lon);
    }
}

public class ProtectedSpeciesDetectionTests
{
    private static Species MakeSpecies(ConservationStatus status) => new()
    {
        CommonNameAz = "x",
        CommonNameEn = "x",
        LatinName = "x",
        HabitatType = HabitatType.River,
        MinSizeCm = 1,
        MaxSizeCm = 10,
        Diet = "x",
        ConservationStatus = status,
        BestBaitTechnique = "x",
        LegalSeasonNotes = "x"
    };

    private static bool IsProtected(Species species) => species.ConservationStatus >= ConservationStatus.Vulnerable;

    [Theory]
    [InlineData(ConservationStatus.LeastConcern, false)]
    [InlineData(ConservationStatus.NearThreatened, false)]
    [InlineData(ConservationStatus.Vulnerable, true)]
    [InlineData(ConservationStatus.Endangered, true)]
    [InlineData(ConservationStatus.CriticallyEndangered, true)]
    public void IsProtectedSpeciesCatch_MatchesExpectedThreshold(ConservationStatus status, bool expectedProtected)
    {
        var species = MakeSpecies(status);

        var result = IsProtected(species);

        Assert.Equal(expectedProtected, result);
    }
}