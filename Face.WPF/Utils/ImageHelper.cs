using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Face.WPF.Utils
{
    public class ImageHelper
    {
        #region 通常是Winform使用 System.Drawing.Image
        /// <summary>
        /// 图像转16进制
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static string ImageToHex(Image image)
        {
            using (var ms = new MemoryStream())
            {
                // 将图片保存到内存流中
                image.Save(ms, image.RawFormat);
                byte[] imageBytes = ms.ToArray();
                // 将字节转换为十六进制字符串
                StringBuilder hex = new StringBuilder(imageBytes.Length * 2);
                foreach (byte b in imageBytes)
                    hex.AppendFormat("{0:x2}", b);
                ms.Dispose();
                return hex.ToString();
            }
        }

        /// <summary>
        /// 十六进制字符串转为图像
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static Image StrHexToByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
            {
                string temp = hexString.Substring(i * 2, 2).Trim();
                returnBytes[i] = Convert.ToByte(temp, 16);
            }

            MemoryStream stream = new MemoryStream(returnBytes);
            Image image = Image.FromStream(stream);

            return image;
        }
        #endregion

        #region 通常是WPF使用 System.Windows.Controls.Image
        public static string ImageToHex(BitmapImage bitmapImage)
        {
            if (bitmapImage == null)
                throw new ArgumentNullException(nameof(bitmapImage));

            // 将 BitmapImage 转换为 BitmapSource
            BitmapSource bitmapSource = bitmapImage as BitmapSource;

            // 获取图片的像素数据
            int stride = (bitmapSource.PixelWidth * bitmapSource.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[bitmapSource.PixelHeight * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);

            // 将像素数据转换为十六进制字符串
            return BitConverter.ToString(pixels).Replace("-", "");
        }

        public static BitmapImage StrHexToByte(string hexString, int width, int height)
        {
            if (string.IsNullOrEmpty(hexString))
                throw new ArgumentNullException(nameof(hexString));

            // 将十六进制字符串转换为字节数组
            byte[] imageBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < imageBytes.Length; i++)
            {
                imageBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            // 创建 BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(imageBytes))
            {
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.DecodePixelWidth = width;
                bitmapImage.DecodePixelHeight = height;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
        #endregion

        public static BitmapImage ConvertToBitmapImage(Bitmap bitmap, ImageFormat? imageFormat = null)
        {
            if (imageFormat == null)
                imageFormat = ImageFormat.Png;

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, imageFormat);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static BitmapImage ConvertToBitmapImage(byte[] imageBytes)
        {
            using (MemoryStream memory = new MemoryStream(imageBytes))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption |= BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
