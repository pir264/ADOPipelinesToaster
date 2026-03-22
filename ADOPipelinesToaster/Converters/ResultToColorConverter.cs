using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ADOPipelinesToaster.Converters;

public class ResultToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value as string)?.ToLower() switch
        {
            "succeeded" => new SolidColorBrush(Color.FromRgb(76, 153, 0)),
            "failed" => new SolidColorBrush(Color.FromRgb(196, 43, 28)),
            "canceled" => new SolidColorBrush(Color.FromRgb(130, 130, 130)),
            "partiallysucceeded" => new SolidColorBrush(Color.FromRgb(204, 130, 0)),
            _ => new SolidColorBrush(Color.FromRgb(80, 80, 80))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
