using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics.Tracing;
using Face.WPF.Utils;
using System.IO.Ports;
using Face.WPF.ViewModels;
using Face.WPF.Views;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Controls;

namespace Face.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel.MainWindow = this;

            this.UpBtn.Click += (s, e) =>
            {
                SolidColorBrush brush = (SolidColorBrush)FindResource("BackgroupFour");
                this.Topmost = this.Topmost ? false : true;
                UpBtn.Foreground = this.Topmost ? brush : this.CloseBtn.Foreground;
            };
            this.MinBtn.Click += (s, e) => this.WindowState = WindowState.Minimized ;
            this.MaxBtn.Click += (s, e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            this.CloseBtn.Click += (s, e) => Application.Current.Shutdown();
            this.SetBtn.Click += (s, e) =>
            {
                SolidColorBrush brush = (SolidColorBrush)FindResource("BackgroupFour");
                ScrView.Visibility = ScrView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                this.SetBtn.Foreground = ScrView.Visibility == Visibility.Visible ? brush : this.CloseBtn.Foreground;
                if (this.contentControl.Content is LoginView)
                    this.Width = this.Width == 330 ? 520 : 330;
            };

            serialPort.ItemsSource = SerialPort.GetPortNames();
            baudRate.ItemsSource = new int[] { 110, 300, 600, 1200, 2400, 4800, 9600, 11440, 19200, 38400, 57600, 115200, 128000 }.Select(s => s.ToString());
            parity.ItemsSource = Enum.GetNames(typeof(Parity));
            stopBits.ItemsSource = Enum.GetNames(typeof(StopBits));
            dataBits.ItemsSource = new int[] { 5, 6, 7, 8 }.Select(s => s.ToString());
            handshake.ItemsSource = Enum.GetNames(typeof(Handshake));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Gl.closeVideo();
            Gl.MySerialPort?.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                ToolTip toolTip = button.ToolTip as ToolTip;
                if (toolTip != null)
                {
                    toolTip.IsOpen = false;
                }
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LogOutTBtn.IsChecked = false;
        }
    }
}
