using System;
using System.Globalization;
using System.Windows.Data;

namespace GamefinderVisualizer
{
    public sealed class BoolToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "Visible";
        public string FalseValue { get; set; } = "Hidden";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool) return TrueValue;
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value, TrueValue);
        }

        #endregion
    }
}
