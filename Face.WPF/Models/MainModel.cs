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
    public class MainModel
    {
        public static int imageRotateFlipIndex = 5;
        public static byte[]? imageBytes;

        //官方枚举有重复
        private static RotateFlipType[] rotateFlipTypes = Enum.GetValues(typeof(RotateFlipType))
                                            .Cast<RotateFlipType>()
                                            .Distinct()
                                            .OrderBy(x => x)
                                            .ToArray();

        public static void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            VideoCaptureDevice? device = sender as VideoCaptureDevice;
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            bitmap.RotateFlip(rotateFlipTypes[imageRotateFlipIndex]);
            if (Gl.isCapture)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    imageBytes = memoryStream.ToArray();
                }
                Gl.isCapture = false;
            }
            Gl.showImage?.Invoke(bitmap);  
        }
    }
}
