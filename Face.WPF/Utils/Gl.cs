using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.IO.Ports;
using Face.WPF.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace Face.WPF.Utils
{
    public class Gl
    {
        public static bool windowClose = false;
        public static int faceID = -1;
        public static int faceUserID = -1;
        public static int scanFaceTimeOut = 10;
        public static byte faseReplyRes = 0x00;
        public static int faseState = 0;
        public static bool isCapture = false;

        public static Action closeVideo;
        public static Action<Bitmap> showImage;


        public static Action<string> PrintLog { get; set; } = new Action<string>(str => { });
        public static Action<string, System.Windows.Media.Brush?, System.Windows.Media.Brush?> PrintLogColor { get; set; } = new Action<string, System.Windows.Media.Brush?, System.Windows.Media.Brush?>((str, b, f) => { });

        public static SerialPort MySerialPort { get; set; }
        public static Action<int> SerialWrite { get; set; }
        public static Action<UserModel> Login { get; set; }
    }
}
