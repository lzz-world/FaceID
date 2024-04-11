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
        static Color borderColor = Color.Yellow;
        static Color starColor = Color.Red;

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

            if (!Gl.IsStartVedio) return;
            Gl.showImage?.Invoke(EditBitmap(bitmap));
        }

        private static Bitmap EditBitmap(Bitmap bitmap)
        {
            #region 压缩图像
            // 创建一个新的 Bitmap 对象，并设置其大小为压缩后的宽度和高度
            Bitmap compressedImage = new Bitmap(Gl.ImageWidth, Gl.ImageHeight);

            // 创建一个绘图对象，用于绘制压缩后的图像
            using (Graphics graphics = Graphics.FromImage(compressedImage))
            {
                // 设置绘图对象的插值模式以提高压缩质量
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // 将原始图像绘制到压缩后的图像上
                graphics.DrawImage(bitmap, 0, 0, Gl.ImageWidth, Gl.ImageHeight);
            }
            #endregion

            if (Gl.ImageWidth >= 320 && Gl.ImageHeight >= 480)
            {       
                // 固定轮廓框
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        compressedImage.SetPixel(80 - k, 120 + i, borderColor);
                        compressedImage.SetPixel(80 - k, 360 - i, borderColor);
                        compressedImage.SetPixel(240 + k, 120 + i, borderColor);
                        compressedImage.SetPixel(240 + k, 360 - i, borderColor);
                        
                        compressedImage.SetPixel(80 + i, 120 - k, borderColor);
                        compressedImage.SetPixel(240 - i, 120 - k, borderColor);
                        compressedImage.SetPixel(80 + i, 360 + k, borderColor);
                        compressedImage.SetPixel(240 - i, 360 + k, borderColor);
                    }
                }

                // 面部描点
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

                        compressedImage.SetPixel(x, Gl.fasePos[2], starColor);
                        compressedImage.SetPixel(Gl.fasePos[1], y, starColor);
                    }
                }
            }

            if (!isAutoClearPos && Gl.fasePos[0] == 0)
            {
                isAutoClearPos = true;
                Task.Run(async () =>
                {
                    await Task.Delay(10000);
                    Gl.fasePos = new int[] { -1 };
                    isAutoClearPos = false;
                });
            }

            return compressedImage;
        }
    }
}
