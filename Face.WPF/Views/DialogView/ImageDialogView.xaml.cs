using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Face.WPF.Views.DialogView
{
    /// <summary>
    /// ImageDialogView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageDialogView : Window
    {
        private ImageDialogView()
        {
            
        }

        public ImageDialogView(ImageSource imageSource)
        {
            InitializeComponent();

            this.BtnClose.Click += (s, e) => this.Close();
            image.Source = imageSource;
        }
    }
}
