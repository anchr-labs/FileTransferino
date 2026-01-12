using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace FileTransferino.App.Converters
{
    public class BoolInverseConverter : IValueConverter
    {
        public static readonly BoolInverseConverter Instance = new BoolInverseConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return value == null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;
            return value == null;
        }
    }
}
