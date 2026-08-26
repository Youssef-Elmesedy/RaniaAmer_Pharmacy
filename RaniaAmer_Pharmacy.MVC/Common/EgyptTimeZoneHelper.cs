namespace RaniaAmer_Pharmacy.MVC.Common;

/// <summary>
/// All timestamps are stored in the database as UTC (via DateTime.UtcNow) — that's the correct
/// way to store them. This helper converts to Egypt local time only at DISPLAY time, so users
/// and admins always see times that match their clock, regardless of where the server runs.
/// </summary>
public static class EgyptTimeZoneHelper
{
    private static readonly TimeZoneInfo EgyptTimeZone = ResolveEgyptTimeZone();

    private static TimeZoneInfo ResolveEgyptTimeZone()
    {
        // Windows and Linux/containers use different time zone ID naming schemes, so try both,
        // and fall back to a fixed UTC+2 offset (Egypt has not observed DST since 2016) if the
        // OS has no time zone database entry for Egypt at all.
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); // Windows
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); // Linux / IANA
        }
        catch (TimeZoneNotFoundException)
        {
        }
        catch (InvalidTimeZoneException)
        {
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            id: "Egypt_Fixed_UTC+2",
            baseUtcOffset: TimeSpan.FromHours(2),
            displayName: "(UTC+02:00) Egypt",
            standardDisplayName: "Egypt Standard Time");
    }

    /// <summary>
    /// Converts a stored UTC DateTime to Egypt local time for display. If the value's Kind is
    /// Unspecified (common for values read back from SQL Server / EF Core), it's treated as UTC
    /// since that's how this app always writes timestamps (DateTime.UtcNow).
    /// </summary>
    public static DateTime ToEgyptTime(this DateTime utcDateTime)
    {
        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, EgyptTimeZone);
    }

    public static DateTime? ToEgyptTime(this DateTime? utcDateTime) =>
        utcDateTime.HasValue ? utcDateTime.Value.ToEgyptTime() : null;
}
