using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace FileTransferino.App.Converters
{
    /// <summary>
    /// Value converter that inverts boolean values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <paramref name="value"/> is a <see cref="bool"/>, the logical negation is returned.
    /// </para>
    /// <para>
    /// If the <paramref name="value"/> is <c>null</c>, this converter returns <c>true</c>.
    /// This treats a missing/undefined value as <c>false</c> and then applies the inversion,
    /// which is useful for bindings where an absent source value should enable a control.
    /// </para>
    /// </remarks>
    public class BoolInverseConverter : IValueConverter
    {
        public static readonly BoolInverseConverter Instance = new BoolInverseConverter();

        /// <summary>
        /// Inverts a boolean value for binding from source to target.
        /// </summary>
        /// <param name="value">The source value to convert. If a <see cref="bool"/>, it is negated; if <c>null</c>, <c>true</c> is returned.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// The negated boolean when <paramref name="value"/> is a <see cref="bool"/>;
        /// otherwise <c>true</c> when <paramref name="value"/> is <c>null</c>;
        /// or <c>false</c> for any other non-boolean value.
        /// </returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            // For non-boolean values, treat a missing value (null) as false and then invert to true.
            return value == null;
        }

        /// <summary>
        /// Inverts a boolean value for binding from target back to source.
        /// </summary>
        /// <param name="value">The target value to convert back. If a <see cref="bool"/>, it is negated; if <c>null</c>, <c>true</c> is returned.</param>
        /// <param name="targetType">The type of the source property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// The negated boolean when <paramref name="value"/> is a <see cref="bool"/>;
        /// otherwise <c>true</c> when <paramref name="value"/> is <c>null</c>;
        /// or <c>false</c> for any other non-boolean value.
        /// </returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return !b;

            // For non-boolean values, treat a missing value (null) as false and then invert to true.
            return value == null;
        }
    }
}
