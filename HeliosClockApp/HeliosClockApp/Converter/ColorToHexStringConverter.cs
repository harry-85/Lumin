using HeliosClockCommon.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace HeliosClockApp.Converter
{
    public class ColorToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ColorHelpers.HexConverter((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ColorHelpers.FromHex((string)value);
        }
    }
}
