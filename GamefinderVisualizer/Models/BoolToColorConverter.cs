using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GamefinderVisualizer
{
    public sealed class BoolToColorConverter : IValueConverter
    {
        public Brush TrueColor { get; set; } = Brushes.LightBlue;
        public Brush FalseColor { get; set; } = Brushes.Yellow;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool) return TrueColor;
            return (bool)value ? TrueColor : FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Brush) return false;
            return ReferenceEquals((Brush)value, TrueColor);
        }

        #endregion
    }
}
