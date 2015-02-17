using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Markup;

namespace FeedbinWP.Converters
{
    public class StringToColor
    {
        public static Color convert(String strColor)
        {
            string xaml = string.Format("<Color xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">{0}</Color>", strColor);
            try
            {
                object obj = XamlReader.Load(xaml);
                if (obj != null && obj is Color)
                {
                    return (Color)obj;
                }
            }
            catch (Exception)
            {
                //Swallow useless exception
            }
            return new Color();
        }
    }
}
