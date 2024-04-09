using Face.WPF.Utils;
using Face.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace Face.WPF.ViewModels
{
    public partial class VedioViewModel : ObservableObject
    {
        private long markTicks;

        [ObservableProperty] private BitmapImage imageSource;

        public VedioViewModel()
        {
            int pointCount = 10;
            Gl.showImage = bitmap =>
            {
                //固定轮廓框
                for (int i = 0; i < 20; i++)
                {
                    bitmap.SetPixel(80, 120 + i, System.Drawing.Color.White);
                    bitmap.SetPixel(80, 340 + i, System.Drawing.Color.White);
                    bitmap.SetPixel(240, 120 + i, System.Drawing.Color.White);
                    bitmap.SetPixel(240, 340 + i, System.Drawing.Color.White);

                    bitmap.SetPixel(80 + i, 120, System.Drawing.Color.White);
                    bitmap.SetPixel(220 + i, 120, System.Drawing.Color.White);
                    bitmap.SetPixel(80 + i, 360, System.Drawing.Color.White);
                    bitmap.SetPixel(220 + i, 360, System.Drawing.Color.White);
                }

                if (Gl.fasePos.Sum() - Gl.fasePos[0] > 0)
                {
                    markTicks = DateTime.Now.Ticks;

                    //图像大小限制480*320, 可模块返回值有超出此大小需要判断。只用到左上角坐标
                    Gl.fasePos[1] = Gl.fasePos[1] > 319 ? (short)319 : Gl.fasePos[1];
                    Gl.fasePos[2] = Gl.fasePos[2] > 479 ? (short)479 : Gl.fasePos[2];

                    for (int i = 0; i < pointCount; i++)
                    {
                        int w = Gl.fasePos[1] - pointCount / 2 <= 1 ? 0 : pointCount / 2;
                        int h = Gl.fasePos[2] - pointCount / 2 <= 1 ? 0 : pointCount / 2;

                        int x = Gl.fasePos[1] - w + i > 319 ? 319 : Gl.fasePos[1] - w + i;
                        int y = Gl.fasePos[2] - w + i > 479 ? 479 : Gl.fasePos[2] - h + i;

                        bitmap.SetPixel(x, Gl.fasePos[2], System.Drawing.Color.Red);
                        bitmap.SetPixel(Gl.fasePos[1], y, System.Drawing.Color.Red);
                    }
                }

                if (DateTime.Now.Ticks - markTicks > 10_000_0000)
                {
                    markTicks = DateTime.Now.Ticks;
                    Gl.fasePos = new int[8];
                }

                Application.Current.Dispatcher.BeginInvoke(new Action(() => ImageSource = ImageHelper.ConvertToBitmapImage(bitmap)));
            };
        }
    }
}
