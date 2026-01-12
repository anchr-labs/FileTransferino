using System.Globalization;
using Avalonia.Data.Converters;

namespace FileTransferino.App.Converters

{
    /// <summary>
    /// Converts <see cref="bool"/> values to their logical inverse and back.
    /// </summary>
    /// <remarks>
    /// When <paramref name="value"/> is <c>null</c>, it is treated as <c>false</c> and therefore
    /// inverted to <c>true</c>. This is useful for bindings where an unset or missing boolean
    /// value should enable a control by default.
    /// </remarks>
    public class BoolInverseConverter : IValueConverter
    {
        public static readonly BoolInverseConverter Instance = new();

        /// <summary>
        /// Inverts a <see cref="bool"/> value. If the input is <c>null</c>, it is treated as
        /// <c>false</c> and the method returns <c>true</c>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The inverted boolean value, or <c>true</c> when <paramref name="value"/> is <c>null</c>.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            // Treat a null source value as false and invert it to true.
            return value == null;
        }

        /// <summary>
        /// Inverts a <see cref="bool"/> value back to the source. If the input is <c>null</c>,
        /// it is treated as <c>false</c> and the method returns <c>true</c>.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">An optional parameter to be used in the converter.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The inverted boolean value, or <c>true</c> when <paramref name="value"/> is <c>null</c>.</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            // Treat a null target value as false and invert it to true.

            // For non-boolean values, treat a missing value (null) as false and then invert to true.
            return value == null;
        }
    }
}
