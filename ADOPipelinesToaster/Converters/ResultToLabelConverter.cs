using System;
using System.Globalization;
using System.Windows.Data;

namespace ADOPipelinesToaster.Converters;

public class ResultToLabelConverter : IValueConverter
{
    private static readonly System.Collections.Generic.HashSet<string> KnownResults =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "succeeded", "failed", "canceled", "partiallysucceeded"
        };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var result = value as string;
        if (string.IsNullOrEmpty(result) || KnownResults.Contains(result))
            return string.Empty;
        return $": {result}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
