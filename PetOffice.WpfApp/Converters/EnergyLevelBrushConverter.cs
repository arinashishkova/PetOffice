using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PetOffice.WpfApp.Converters
{
    public class EnergyLevelBrushConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value is not int energy)
            {
                return CreateBrush("#B8C0C8");
            }

            return energy switch
            {
                >= 91 => CreateBrush("#43A95E"),
                >= 81 => CreateBrush("#5DB76A"),
                >= 71 => CreateBrush("#79C36A"),
                >= 61 => CreateBrush("#9BC96A"),
                >= 51 => CreateBrush("#C0CB63"),
                >= 41 => CreateBrush("#D8C45A"),
                >= 31 => CreateBrush("#E6AE52"),
                >= 21 => CreateBrush("#E8904F"),
                >= 11 => CreateBrush("#E26E50"),
                _ => CreateBrush("#D75252")
            };
        }


        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }


        private static SolidColorBrush CreateBrush(
            string color)
        {
            return new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(color));
        }
    }
}