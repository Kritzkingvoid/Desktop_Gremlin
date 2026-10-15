using System;
using System.Globalization;

// Config files use ungrouped numbers. Accept decimal commas written by older
// Options builds, but never interpret a decimal separator as a thousands separator.
public static class ConfigNumbers
{
    public static bool TryParseDouble(string input, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        string normalized = input.Trim().Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
            || double.IsNaN(value) || double.IsInfinity(value))
        {
            return false;
        }

        result = value;
        return true;
    }

    public static bool TryParseScale(string input, out double result)
    {
        return TryParseDouble(input, out result) && result > 0;
    }

    public static bool TryParseInt(string input, out int result)
    {
        return int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }
}
