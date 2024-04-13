using Face.WPF.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Face.WPF.Utils.Converters
{
    class UserInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string res = value.ToString();
            if (res == GenderType.Men.ToString())
                return "男";
            else if (res == GenderType.Women.ToString())
                return "女";
            else if (res     == UserType.Admin.ToString())
                return "管理员";
            else if (res == UserType.Operator.ToString())
                return "操作员";
            else if (res == UserType.Maintain.ToString())
                return "维护员";
            else if (res == UserType.Visitor.ToString())
                return "访客";
            else if (res == DepartmentType.ProduceOne.ToString())
                return "生产一部";
            else if (res == DepartmentType.ProduceTwo.ToString())
                return "生产二部";
            else if (res == DepartmentType.ProduceThree.ToString())
                return "生产三部";
            else return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
