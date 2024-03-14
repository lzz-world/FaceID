using AForge.Video;
using AForge.Video.DirectShow;
using Face.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Face.WPF.Models
{
    internal class MainModel
    {
        public static int imageRotateFlipIndex = 1;

        //官方枚举有重复
        private static RotateFlipType[] rotateFlipTypes = Enum.GetValues(typeof(RotateFlipType))
                                            .Cast<RotateFlipType>()
                                            .Distinct()
                                            .OrderBy(x => x)
                                            .ToArray();

        public static BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            VideoCaptureDevice? device = sender as VideoCaptureDevice;
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            bitmap.RotateFlip(rotateFlipTypes[imageRotateFlipIndex]);
            //BitmapImage bitmapImage = ConvertToBitmapImage(bitmap);

            Gl.showImage?.Invoke(bitmap);
        }
    }
}
