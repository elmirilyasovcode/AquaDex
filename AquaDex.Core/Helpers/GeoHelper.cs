using System.Linq.Expressions;

namespace AquaDex.Core.Helpers;

public static class GeoHelper
{
    private const double EarthRadiusKm = 6371.0;

    public static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

    /// <summary>
    /// Filters a list of items to those within radiusKm of a given point, sorted nearest-first.
    /// getLat/getLon extract coordinates from each item (as double).
    /// </summary>
    public static List<T> FilterByRadius<T>(
        IEnumerable<T> items,
        double centerLat,
        double centerLon,
        double radiusKm,
        Func<T, double> getLat,
        Func<T, double> getLon)
    {
        return items
            .Select(item => new
            {
                Item = item,
                Distance = CalculateDistanceKm(centerLat, centerLon, getLat(item), getLon(item))
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Select(x => x.Item)
            .ToList();
    }
}