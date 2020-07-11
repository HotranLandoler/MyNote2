using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyNote2
{
    /// <summary>
    /// TitleInput.xaml 的交互逻辑
    /// </summary>
    public partial class TitleInput : Window
    {
        private MainWindow mainWindow;
        public TitleInput(MainWindow main)
        {
            InitializeComponent();
            this.mainWindow = main;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.titleInput = null;
            DialogResult = false;
        }

        /// <summary>
        /// OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string text = txtInput.Text;
            Regex illegal = new Regex(".*[\\\\/:*?\"<>\\|]+.*");
            if (illegal.IsMatch(text) || string.IsNullOrEmpty(text))
            {
                //不合法标题
                txtInput.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }
            foreach (string fileName in mainWindow.GetNoteList())
            {
                if (fileName.Equals(text))
                {
                    //重名
                    txtInput.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
            }
            mainWindow.titleInput = text;
            DialogResult = true;
        }
    }
}
