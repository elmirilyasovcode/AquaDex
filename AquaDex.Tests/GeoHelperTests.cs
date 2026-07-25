using AquaDex.Core.Helpers;
using Xunit;

namespace AquaDex.Tests;

public class GeoHelperTests
{
    [Fact]
    public void CalculateDistanceKm_SamePoint_ReturnsZero()
    {
        var distance = GeoHelper.CalculateDistanceKm(40.4093, 49.8671, 40.4093, 49.8671);
        Assert.Equal(0, distance, precision: 3);
    }

    [Fact]
    public void CalculateDistanceKm_BakuToGanja_ReturnsApproximatelyCorrectDistance()
    {
        // Baku and Ganja are roughly 300km apart in reality
        var distance = GeoHelper.CalculateDistanceKm(40.4093, 49.8671, 40.6828, 46.3606);

        Assert.True(distance > 280 && distance < 320,
            $"Expected distance between 280-320km, but got {distance}km");
    }

    [Fact]
    public void FilterByRadius_ExcludesPointsOutsideRadius()
    {
        var points = new List<(double lat, double lon, string name)>
        {
            (40.4093, 49.8671, "Baku"),
            (40.6828, 46.3606, "Ganja")
        };

        var nearby = GeoHelper.FilterByRadius(
            points,
            centerLat: 40.4093,
            centerLon: 49.8671,
            radiusKm: 25,
            getLat: p => p.lat,
            getLon: p => p.lon
        );

        Assert.Single(nearby);
        Assert.Equal("Baku", nearby[0].name);
    }

    [Fact]
    public void FilterByRadius_IncludesAllPointsWithLargeRadius()
    {
        var points = new List<(double lat, double lon, string name)>
        {
            (40.4093, 49.8671, "Baku"),
            (40.6828, 46.3606, "Ganja")
        };

        var nearby = GeoHelper.FilterByRadius(
            points,
            centerLat: 40.4093,
            centerLon: 49.8671,
            radiusKm: 400,
            getLat: p => p.lat,
            getLon: p => p.lon
        );

        Assert.Equal(2, nearby.Count);
    }
}