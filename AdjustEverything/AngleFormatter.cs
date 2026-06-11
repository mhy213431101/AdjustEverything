using System.Globalization;

namespace AdjustEverything;

internal static class AngleFormatter
{
    public static string ToDms(double degrees)
    {
        if (double.IsNaN(degrees) || double.IsInfinity(degrees))
        {
            return "";
        }

        var sign = degrees < 0 ? "-" : "";
        var value = Math.Abs(degrees);

        var d = (int)Math.Floor(value);
        var minuteValue = (value - d) * 60.0;
        var m = (int)Math.Floor(minuteValue);
        var s = Math.Round((minuteValue - m) * 60.0, 2, MidpointRounding.AwayFromZero);

        if (s >= 60.0)
        {
            s -= 60.0;
            m++;
        }

        if (m >= 60)
        {
            m -= 60;
            d++;
        }

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{sign}{d:00}°{m:00}'{s:00.##}\"");
    }

    public static bool TryParseDms(string? text, out double degrees)
    {
        degrees = 0.0;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim();

        if (TryParseDouble(trimmed, out degrees))
        {
            return true;
        }

        var normalized = trimmed
            .Replace("°", " ")
            .Replace("º", " ")
            .Replace("度", " ")
            .Replace("'", " ")
            .Replace("′", " ")
            .Replace("’", " ")
            .Replace("\"", " ")
            .Replace("″", " ")
            .Replace("”", " ")
            .Replace(":", " ");

        var parts = normalized
            .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length is < 2 or > 3)
        {
            return false;
        }

        if (!TryParseDouble(parts[0], out var d) ||
            !TryParseDouble(parts[1], out var m))
        {
            return false;
        }

        var s = 0.0;
        if (parts.Length == 3 && !TryParseDouble(parts[2], out s))
        {
            return false;
        }

        if (m < 0 || m >= 60 || s < 0 || s >= 60)
        {
            return false;
        }

        var sign = d < 0 ? -1.0 : 1.0;
        degrees = sign * (Math.Abs(d) + m / 60.0 + s / 3600.0);
        return true;
    }

    private static bool TryParseDouble(string text, out double value)
    {
        return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
