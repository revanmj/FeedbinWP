using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FeedbinWP.Converters
{
    public class UnreadToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, Object parameter, String language)
        {
            bool read = (bool)value;
            if (read)
            
                return new SolidColorBrush(StringToColor.convert("#99FFFFFF"));
            else
                return new SolidColorBrush(StringToColor.convert("#FFFFFFFF"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
