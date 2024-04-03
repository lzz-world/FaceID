using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Face.WPF.Utils.Converters
{
    class DataGridHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == "注册" && parameter.ToString() == "Face")
                return "人脸注册";
            else if (value.ToString() == "更新" && parameter.ToString() == "Face")
                return "人脸更新";
            else if (value.ToString() == "注册" && parameter.ToString() == "Account")
                return "账号注册";
            else if (value.ToString() == "更新" && parameter.ToString() == "Account")
                return "账号更新";
            else return "NUll";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
