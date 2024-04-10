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
        private const int POINT_COUNT = 10;
        public static int imageRotateFlipIndex = 5;
        public static byte[]? imageBytes;
        private static bool isAutoClearPos = false;

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

            //固定轮廓框
            for (int i = 0; i < 20; i++)
            {
                bitmap.SetPixel(80, 120 + i, Color.White);
                bitmap.SetPixel(80, 340 + i, Color.White);
                bitmap.SetPixel(240, 120 + i, Color.White);
                bitmap.SetPixel(240, 340 + i, Color.White);

                bitmap.SetPixel(80 + i, 120, Color.White);
                bitmap.SetPixel(220 + i, 120, Color.White);
                bitmap.SetPixel(80 + i, 360, Color.White);
                bitmap.SetPixel(220 + i, 360, Color.White);
            }

            if (Gl.fasePos.Sum() - Gl.fasePos[0] > 0)
            {
                //图像大小限制480*320, 可模块返回值有超出此大小需要判断。只用到左上角坐标
                Gl.fasePos[1] = Gl.fasePos[1] > 319 ? (short)319 : Gl.fasePos[1];
                Gl.fasePos[2] = Gl.fasePos[2] > 479 ? (short)479 : Gl.fasePos[2];

                for (int i = 0; i < POINT_COUNT; i++)
                {
                    int w = Gl.fasePos[1] - POINT_COUNT / 2 <= 1 ? 0 : POINT_COUNT / 2;
                    int h = Gl.fasePos[2] - POINT_COUNT / 2 <= 1 ? 0 : POINT_COUNT / 2;

                    int x = Gl.fasePos[1] - w + i > 319 ? 319 : Gl.fasePos[1] - w + i;
                    int y = Gl.fasePos[2] - w + i > 479 ? 479 : Gl.fasePos[2] - h + i;

                    bitmap.SetPixel(x, Gl.fasePos[2], Color.Red);
                    bitmap.SetPixel(Gl.fasePos[1], y, Color.Red);
                }
            }

            if (!isAutoClearPos && Gl.fasePos[0] == 0)
            {
                isAutoClearPos = true;
                Task.Run(async () => { 
                    await Task.Delay(10000); 
                    Gl.fasePos = new int[] { -1 };
                    isAutoClearPos = false; 
                });
            }

            if (!Gl.IsStartVedio) return;
            Gl.showImage?.Invoke(bitmap);
        }
    }
}
