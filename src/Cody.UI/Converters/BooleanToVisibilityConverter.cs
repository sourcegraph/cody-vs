using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cody.UI.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BooleanToVisibilityConverter Default = new BooleanToVisibilityConverter();
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BooleanToVisibilityConverter Inverted = new BooleanToVisibilityConverter { IsInverted = true };
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BooleanToVisibilityConverter DefaultHidden = new BooleanToVisibilityConverter { HideInsteadOfCollapse = true };
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly BooleanToVisibilityConverter InvertedHidden = new BooleanToVisibilityConverter { IsInverted = true, HideInsteadOfCollapse = true };

        public Boolean IsInverted { get; set; }

        public Boolean HideInsteadOfCollapse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = Visibility.Collapsed;
            if (HideInsteadOfCollapse)
            {
                result = Visibility.Hidden;
            }

            if (value == null)
                return result;

            bool input = true;
            if (value is bool)
                input = (bool)value;
            if ((IsInverted && !input) || (!IsInverted && input))
                result = Visibility.Visible;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
