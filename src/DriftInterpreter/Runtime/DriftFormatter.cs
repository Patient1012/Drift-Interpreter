using System.Globalization;
using System.Text;

public static class DriftFormatter
{
    public static string Format(
        string format,
        IReadOnlyList<object?> arguments,
        SourcePosition location)
    {
        var result = new StringBuilder();
        int argumentIndex = 0;

        for (int i = 0; i < format.Length; i++)
        {
            char current = format[i];

            if (current != '%')
            {
                result.Append(current);
                continue;
            }

            if (i + 1 >= format.Length)
                throw new RuntimeException("Invalid format string: trailing '%'.", location);

            char specifier = format[++i];

            if (specifier == '%')
            {
                result.Append('%');
                continue;
            }

            if (argumentIndex >= arguments.Count)
                throw new RuntimeException(
                    $"Format specifier '%{specifier}' requires an argument.",
                    location);

            result.Append(FormatValue(specifier, arguments[argumentIndex], location));
            argumentIndex++;
        }

        if (argumentIndex < arguments.Count)
            throw new RuntimeException(
                $"Too many arguments for the format string. Expected {argumentIndex}, got {arguments.Count}.",
                location);

        return result.ToString();
    }

    private static string FormatValue(char specifier, object? value, SourcePosition location)
    {
        return specifier switch
        {
            'i' or 'd' => FormatInteger(value, specifier, location),
            'f' => FormatFloat(value, location),
            's' => value as string
                ?? throw new RuntimeException(
                    $"Format specifier '%{specifier}' requires a string argument.",
                    location),
            'c' => value is char character
                ? character.ToString()
                : throw new RuntimeException(
                    $"Format specifier '%{specifier}' requires a char argument.",
                    location),
            'b' => value is bool boolean
                ? (boolean ? "true" : "false")
                : throw new RuntimeException(
                    $"Format specifier '%{specifier}' requires a bool argument.",
                    location),
            'v' => FormatDefault(value),
            _ => throw new RuntimeException(
                $"Unknown format specifier '%{specifier}'.",
                location)
        };
    }

    private static string FormatInteger(object? value, char specifier, SourcePosition location)
    {
        if (!IsIntegral(value))
            throw new RuntimeException(
                $"Format specifier '%{specifier}' requires an integer argument.",
                location);

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatFloat(object? value, SourcePosition location)
    {
        if (!IsNumeric(value))
            throw new RuntimeException(
                "Format specifier '%f' requires a numeric argument.",
                location);

        return ToDouble(value).ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatDefault(object? value)
    {
        return value switch
        {
            null => string.Empty,
            bool boolean => boolean ? "true" : "false",
            string text => text,
            char character => character.ToString(),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static bool IsIntegral(object? value)
    {
        return value is sbyte or short or int or long or byte or ushort or uint or ulong;
    }

    private static bool IsFloating(object? value)
    {
        return value is Half or float or double;
    }

    private static bool IsNumeric(object? value)
    {
        return IsIntegral(value) || IsFloating(value);
    }

    private static double ToDouble(object? value)
    {
        return value switch
        {
            Half half => (double)half,
            float single => single,
            double number => number,
            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture)
        };
    }
}
