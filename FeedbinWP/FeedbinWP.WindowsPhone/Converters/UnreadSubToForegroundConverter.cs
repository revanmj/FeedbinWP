using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FeedbinWP.Converters
{
    class UnreadSubToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, Object parameter, String language)
        {
            bool read = (bool)value;
            if (read)

                return new SolidColorBrush(StringToColor.convert("#99FFFFFF"));
            else
                return new SolidColorBrush(StringToColor.convert("#CCFFFFFF"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
