using Face.WPF.Utils;
using Face.WPF.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace Face.WPF.ViewModels
{
    public partial class VedioViewModel : ObservableObject
    {
        [ObservableProperty] private BitmapImage imageSource;

        public VedioViewModel()
        {
            Gl.showImage = bitmap => Application.Current.Dispatcher.Invoke(() => ImageSource = ImageHelper.ConvertToBitmapImage(bitmap));
        }
    }
}
