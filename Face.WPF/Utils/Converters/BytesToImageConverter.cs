using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Face.WPF.Utils.Converters
{
    internal class BytesToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[]? imageBytes = value as byte[];
            if (imageBytes != null)
            {
                var imageSource = ImageHelper.ConvertToBitmapImage(imageBytes);
                return imageSource;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
