namespace SiretT.Converters {
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public enum LogicOperator {
        AND, OR, NOT, XOR
    }

    /// <summary>
    /// Checks equality of value and the converter parameter.
    /// Returns <see cref="Visibility.Visible"/> if they are equal.
    /// Returns <see cref="Visibility.Collapsed"/> if they are NOT equal.
    /// </summary>
    public class LogicToVisibilityConverter : IMultiValueConverter {
        public LogicOperator LogicOperator { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (!(values[0] is bool) || !(values[1] is bool)) return Visibility.Collapsed;
            switch (LogicOperator) {
                case LogicOperator.AND:
                    return (bool)values[0] & (bool)values[1] ? Visibility.Visible : Visibility.Collapsed;
                case LogicOperator.OR:
                    return (bool)values[0] | (bool)values[1] ? Visibility.Visible : Visibility.Collapsed;
                case LogicOperator.NOT:
                    return !((bool)values[0] & (bool)values[1]) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public static class Converters {
        public static readonly LogicToVisibilityConverter LogicToVisibilityConverter = new LogicToVisibilityConverter();
    }
}