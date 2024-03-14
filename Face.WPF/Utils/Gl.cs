using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.IO.Ports;

namespace Face.WPF.Utils
{
    public class Gl
    {
        public static bool windowClose = false;
        public static Action closeVideo;
        public static Action<Image> showImage;

        public static Action<string> printLog;
        public static Action<string, System.Windows.Media.Brush?, System.Windows.Media.Brush?> printLogColor;

        public static SerialPort MySerialPort { get; set; }
    }
}
