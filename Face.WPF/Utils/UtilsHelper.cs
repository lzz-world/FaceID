using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face.WPF.Utils
{
    public class UtilsHelper
    {
        /// <summary>
        /// 异或操作
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte PerformXOR(byte[] data)
        {
            byte result = 0;
            foreach (byte b in data)
            {
                result ^= b;
            }
            return result;
        }

        /// <summary>
        /// 获取枚举字段描述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T, U>(U state) where T : Enum
        {
            foreach (T element in Enum.GetValues(typeof(T)))
            {
                if (Convert.ToInt32(element) == Convert.ToInt32(state))
                {
                    var descriptionAttribute = element.GetType()
                        .GetField(element.ToString())
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .SingleOrDefault() as DescriptionAttribute;
                    string? description = descriptionAttribute?.Description;
                    return description ?? string.Empty;
                }   
            }
            return string.Empty;
        }
    }
}
