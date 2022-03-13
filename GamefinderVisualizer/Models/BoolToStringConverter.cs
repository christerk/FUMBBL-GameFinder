using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GamefinderVisualizer
{
    public sealed class BoolToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "Visible";
        public string FalseValue { get; set; } = "Hidden";

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return TrueValue;
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Equals(value, TrueValue);
        }

        #endregion
    }
}
