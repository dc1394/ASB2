using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ASB2
{
    /// <summary>
    /// TextBox 添付ビヘイビア
    /// </summary>
    internal sealed class EnumRadioConverter : IValueConverter
    {
        public Object Convert(Object value, System.Type targetType, Object parameter, CultureInfo culture)
        {
            var paramString = parameter as string;
            if (paramString == null)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (!Enum.IsDefined(value.GetType(), paramString))
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            var paramParsed = Enum.Parse(value.GetType(), paramString);

            return (value.Equals(paramParsed));
        }

        public Object ConvertBack(Object value, System.Type targetType, Object parameter, CultureInfo culture)
        {
            var paramString = parameter as string;
            if (paramString == null)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            return Enum.Parse(targetType, paramString);
        }
    }
}
