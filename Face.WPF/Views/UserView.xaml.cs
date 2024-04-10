using Face.WPF.Utils;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Face.WPF.Views
{
    /// <summary>
    /// UserView.xaml 的交互逻辑
    /// </summary>
    public partial class UserView : UserControl
    {
        private MainWindow _mainWindow;

        public UserView(int tabIndex, MainWindow mainWindow)
        {
            InitializeComponent();

            _mainWindow = mainWindow;

            this.tabControl.SelectedIndex = tabIndex;
            if (tabIndex == 0)
                this.tabControl.Tag = "Visible";
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl.SelectedIndex == 0)
            {
                _mainWindow.ScrView.Visibility = Visibility.Visible;
                Gl.IsStartVedio = true;
            }
            else if (tabControl.SelectedIndex == 1)
                Gl.IsStartVedio = true;
            else
            {
                Gl.IsStartVedio = false;
                _mainWindow.ScrView.Visibility = Visibility.Hidden;
            }
        }
    }
}
