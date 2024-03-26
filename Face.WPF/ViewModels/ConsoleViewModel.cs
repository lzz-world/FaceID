using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Face.WPF.Utils;
using Face.WPF.Views;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Face.WPF.ViewModels
{
    public partial class ConsoleViewModel : ObservableObject
    {
        public enum TextType { debug, info, warning, error }

        public ConsoleView ConsoleView { get; set; }

        /// <summary>
        /// 日志文本框滚动条是否在最下方
        /// true:文本框竖直滚动条在文本框最下面时，可以在文本框后追加日志
        /// false:当用户拖动文本框竖直滚动条，使其不在最下面时，即用户在查看旧日志，此时不添加新日志，
        /// </summary>
        public bool IsVerticalScrollBarAtBottom
        {
            get
            {
                bool atBottom = false;

                ConsoleView.RichTextBox.Dispatcher.Invoke(delegate
                {
                    double dVer = ConsoleView.RichTextBox.VerticalOffset;       //获取竖直滚动条滚动位置
                    double dViewport = ConsoleView.RichTextBox.ViewportHeight;  //获取竖直可滚动内容高度
                    double dExtent = ConsoleView.RichTextBox.ExtentHeight;      //获取可视区域的高度

                    atBottom = dVer + dViewport >= dExtent;
                });
                return atBottom;
            }
        }

        public ConsoleViewModel()
        {
            Gl.printLog = InsertMsg;
            Gl.printLogColor = InsertMsg;
        }

        private void InsertMsg(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ConsoleView.FlowDocument.Blocks.Count > 500)
                {
                    // 超过指定行数后，删除第一条日志，防止日志太多造成卡顿
                    ConsoleView.FlowDocument.Blocks.Remove(ConsoleView.FlowDocument.Blocks.FirstBlock);
                }

                Run text = new Run(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:") + DateTime.Now.Millisecond.ToString().PadLeft(3, '0') + "    " + msg)
                {
                    Foreground = Brushes.White,
                    Background = Brushes.Transparent
                };
                Paragraph insert = new Paragraph(text) { Margin = new Thickness(0, 5, 0, 0) };
                ConsoleView.FlowDocument.Blocks.Add(insert);

                if (IsVerticalScrollBarAtBottom)
                {
                    //解决问题：在日志滚动中选中日志时确保焦点都处于最后，看起来舒适
                    ConsoleView.RichTextBox.ScrollToEnd();
                }
            });
        }

        private void InsertMsg(string msg, Brush? foreground = null, Brush? background = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (ConsoleView.FlowDocument.Blocks.Count > 1000)
                {
                    // 超过指定行数后，删除第一条日志，防止日志太多造成卡顿
                    ConsoleView.FlowDocument.Blocks.Remove(ConsoleView.FlowDocument.Blocks.FirstBlock);
                }

                
                Run text = new Run(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:") + DateTime.Now.Millisecond.ToString().PadLeft(3, '0') + "    " + msg)
                {
                    Foreground = foreground ?? Brushes.Black,
                    Background = background ?? Brushes.Transparent
                };
                Paragraph insert = new Paragraph(text) { Margin = new Thickness(0, 5, 0, 0)};
                ConsoleView.FlowDocument.Blocks.Add(insert);

                if (IsVerticalScrollBarAtBottom)
                {
                    //解决问题：在日志滚动中选中日志时确保焦点都处于最后，看起来舒适
                    ConsoleView.RichTextBox.ScrollToEnd();
                }
            });
        }
    }
}
