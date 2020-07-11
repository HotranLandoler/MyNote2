using System;
using System.Collections.Generic;
using System.IO;
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

namespace MyNote2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string notePath = ".\\notes\\";
        public const string suffix = ".txt";

        public string titleInput = null;

        public List<string> listNames;

        public List<FindInfo> findResults;
        public int resIdx;

        //public string lastModTime;
        public MainWindow()
        {
            InitializeComponent();
            Initialize();         
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            findResults = new List<FindInfo>();
            resIdx = 0;
            listNames = GetNoteList();
            if (!Directory.Exists(notePath))
            {
                Directory.CreateDirectory(notePath);
            }
            listNotes.ItemsSource = listNames;
            if (!listNotes.Items.IsEmpty)
                listNotes.SelectedIndex = 0;
            
        }

        /// <summary>
        /// 窗体拖动
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        /// <summary>
        /// 最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 获取笔记标题列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetNoteList()
        {
            List<string> list = new List<string>();
            DirectoryInfo folder = new DirectoryInfo(notePath);
            FileInfo[] files = folder.GetFiles();
            //files = files.OrderBy(s => (s.LastWriteTime.Ticks))
            foreach (FileInfo file in files)
            {
                //去除后缀加入List
                list.Add(file.Name.Remove(file.Name.LastIndexOf('.')));
            }
            return list;        
        }

        /// <summary>
        /// 选中列表项，获取并显示笔记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listNotes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listNotes.SelectedItem == null)
            {
                lblTitle.Content = "";
                lblTime.Content = "";
                txtNote.Text = "";
                return;
            }
                
            string fileName = listNotes.SelectedItem.ToString();
            string filePath = notePath + fileName + suffix;
            string text = null;
            try
            {
                text = File.ReadAllText(filePath);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("ERROR!");
            }
            UpdateLastModTime(new FileInfo(filePath));
            lblTitle.Content = fileName;           
            txtNote.Text = text;
        }

        /// <summary>
        /// 新建笔记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            TitleInput input = new TitleInput(this);
            bool res = input.ShowDialog().GetValueOrDefault();
            if (res == false || string.IsNullOrEmpty(titleInput))
                return;
            string filePath = notePath + titleInput + suffix;
            File.Create(filePath).Close();
            listNames.Add(titleInput);
            listNotes.Items.Refresh();
            listNotes.SelectedItem = titleInput;
            titleInput = null;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {          
            string target = txtSearch.Text;           
            if (findResults.Count == 0)
            {
                if (!Find(target))
                {
                    txtSearch.BorderBrush = new SolidColorBrush(Colors.Red);
                    return;
                }
            }           
            //读取下一个结果
            string filePath = findResults[resIdx].filePath;
            int idx = findResults[resIdx].index;
            resIdx++;
            if (resIdx >= findResults.Count)
                resIdx = 0;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            
            listNotes.SelectedItem = fileName;
            txtNote.Focus();
            txtNote.Select(idx, target.Length);
        }


        public bool Find(string tar)
        {
            if (string.IsNullOrEmpty(tar))
                return false;
            bool found = false;
            string[] filePaths = Directory.GetFiles(notePath);
            foreach (string filePath in filePaths)
            {
                string text = File.ReadAllText(filePath);
                int idx = text.IndexOf(tar);
                while (idx != -1)
                {
                    found = true;
                    findResults.Add(new FindInfo(filePath, idx));
                    idx = text.IndexOf(tar, idx + 1);
                }
            }
            return found;               
        }
        /// <summary>
        /// 保存笔记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (listNotes.SelectedItem == null)
                return;
            string text = txtNote.Text;
            string fileName = listNotes.SelectedItem.ToString();
            string filePath = notePath + fileName + suffix;
            string old = File.ReadAllText(filePath);
            //仅当内容被修改
            if (!old.Equals(text))
            {
                File.WriteAllText(filePath, text);
                UpdateLastModTime(new FileInfo(filePath));
                findResults.Clear();
            }           
        }

        private void UpdateLastModTime(FileInfo fileInfo)
        {
            lblTime.Content = fileInfo.LastWriteTime.ToString();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            findResults.Clear();
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void itemRename_Click(object sender, RoutedEventArgs e)
        {
            string oldName = listNotes.SelectedItem.ToString();
            TitleInput input = new TitleInput(this);
            bool res = input.ShowDialog().GetValueOrDefault();
            if (res == false || string.IsNullOrEmpty(titleInput))
                return;
            string oldPath = notePath + oldName + suffix;
            string newPath = notePath + titleInput + suffix;
            FileInfo file = new FileInfo(oldPath);
            file.MoveTo(newPath);
            listNames.Remove(oldName);
            listNames.Add(titleInput);
            listNotes.Items.Refresh();
            listNotes.SelectedItem = titleInput;
            titleInput = null;
        }

        /// <summary>
        /// 删除笔记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void itemDel_Click(object sender, RoutedEventArgs e)
        {
            ConfirmDialog dialog = new ConfirmDialog();
            bool res = dialog.ShowDialog().GetValueOrDefault();
            if (res == false)
                return;            
            string fileName = listNotes.SelectedItem.ToString();
            string filePath = notePath + fileName + suffix;
            try
            {
                File.Delete(filePath);
            }        
            catch (Exception ex)
            {
                throw ex;
            }
            
            listNames.Remove(fileName);
            listNotes.Items.Refresh();
            listNotes.SelectedIndex = 0;
            findResults.Clear();
        }

        private void Border_GotFocus(object sender, RoutedEventArgs e)
        {
            ResetTxtFindBorder();
        }

        private void txtNote_GotFocus(object sender, RoutedEventArgs e)
        {
            ResetTxtFindBorder();
        }

        private void ResetTxtFindBorder()
        {
            txtSearch.BorderBrush = new SolidColorBrush(Colors.SlateGray);
        }
    }
}
