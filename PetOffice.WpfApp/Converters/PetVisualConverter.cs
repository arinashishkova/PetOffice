using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PetOffice.Core.Models;
using PetOffice.WpfApp.Presentation;

namespace PetOffice.WpfApp.Converters
{
    public sealed class PetVisualConverter : IValueConverter
    {
        private sealed record PetVisual(string Role, string IconPath, Brush Background);

        private static Brush ColorBrush(string color) =>
            (Brush)new BrushConverter().ConvertFromString(color)!;

        private static readonly Dictionary<Type, PetVisual> Pets = new()
        {
            [typeof(CatManager)] = new("Cat Manager", AppIconPaths.Cat, ColorBrush("#FFF4E8")),
            [typeof(DogIntern)] = new("Dog Intern", AppIconPaths.Dog, ColorBrush("#F5F0FA")),
            [typeof(HamsterIT)] = new("Hamster IT", AppIconPaths.Hamster, ColorBrush("#FFF8EC")),
            [typeof(ParrotReceptionist)] = new("Parrot Receptionist", AppIconPaths.Parrot, ColorBrush("#EFF8F2")),
            [typeof(RabbitAccountant)] = new("Rabbit Accountant", AppIconPaths.Rabbit, ColorBrush("#FDEFF5")),
            [typeof(RaccoonBarista)] = new("Raccoon Barista", AppIconPaths.Raccoon, ColorBrush("#FBEFE3")),
            [typeof(OwlSysAdmin)] = new("Owl SysAdmin", AppIconPaths.Owl, ColorBrush("#EEF0F8"))
        };

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string field = parameter as string ?? "";

            if (field == "GroupIcon")
            {
                return value switch
                {
                    OfficeGroups.PawformanceHub => AppIconPaths.PawformanceHub,
                    OfficeGroups.CreativeCorner => AppIconPaths.CreativeCorner,
                    OfficeGroups.TechDen => AppIconPaths.TechDen,
                    _ => null
                };
            }

            if (value is not OfficePet pet || !Pets.TryGetValue(pet.GetType(), out var visual))
            {
                return DependencyProperty.UnsetValue;
            }

            return field switch
            {
                "Icon" => visual.IconPath,
                "Role" => visual.Role,
                "Background" => visual.Background,
                _ => DependencyProperty.UnsetValue
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}