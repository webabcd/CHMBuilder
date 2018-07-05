using CHMBuilder.Core;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace CHMBuilder
{
    public partial class MainWindow : Window
    {
        private bool _readyDefaultChmItem = false;

        public MainWindow()
        {
            InitializeComponent();

            txtChmTitle.Text = "我的文档";
            txtHtmlDirectory.Text = Config.WorkDirectoryHtmlSample;
        }

        private async void btnBuildChm_Click(object sender, RoutedEventArgs e)
        {
            var chmTitle = txtChmTitle.Text.Trim();
            var htmlDirectory = txtHtmlDirectory.Text.Trim();
            if (string.IsNullOrEmpty(chmTitle) || string.IsNullOrEmpty(htmlDirectory))
            {
                MessageBoxResult result = MessageBox.Show("chm 标题和 html 目录不能为空", "提示", MessageBoxButton.OK);
                return;
            }

            string btnBuildChmText = (string)btnBuildChm.Content;
            btnBuildChm.IsEnabled = false;
            btnBuildChm.Content = "开始生成 chm 文件，可能需要较长时间，请耐心等待。。。";

            try
            {
                string chmPath = await BuildChmAsync(chmTitle, htmlDirectory);

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.FileName = txtChmTitle.Text.Trim();
                sfd.Title = "Save File";
                sfd.Filter = "chm file|*.chm";
                if (sfd.ShowDialog() == true)
                {
                    File.Copy(chmPath, sfd.FileName, true);

                    MessageBox.Show("生成成功");
                }
                else
                {
                    // MessageBox.Show("取消保存");
                }
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.ToString(), "提示", MessageBoxButton.OK);
            }
            finally
            {
                btnBuildChm.IsEnabled = true;
                btnBuildChm.Content = btnBuildChmText;
            }
        }

        private async Task<string> BuildChmAsync(string chmTitle, string htmlDirectory)
        {
            Task<string> task = Task.Run<string>(() =>
            {
                _readyDefaultChmItem = false;

                // hhc.exe 对 html 的路径的长度有要求，如果路径太长，生成 chm 时会报错 Assertion failure: (pszTmp == m_pCompiler->m_pHtmlMem->psz)
                // 为了尽量避免路径太长，所以本例会把 html 都拷贝到指定目录，然后用相对路径，尽量减少路径的长度
                Directory.Delete(Config.WorkDirectoryHtml, true);
                Helper.CopyDirectory(htmlDirectory, Config.WorkDirectoryHtml);

                var chmItemList = GetChmItemlList(Config.WorkDirectoryHtml);
                ChmHelper chmHelper = new ChmHelper(chmTitle, chmItemList);

                return chmHelper.SaveChm();
            });

            return await task;
        }

        private List<ChmItem> GetChmItemlList(string htmlDirectoryPath)
        {
            var chmItemList = new List<ChmItem>();

            var htmlDirectory = new DirectoryInfo(htmlDirectoryPath);
            var files = htmlDirectory.GetFiles("*.html", SearchOption.TopDirectoryOnly);
            var dirs = htmlDirectory.GetDirectories();

            foreach (var fileInfo in files)
            {
                if (dirs.Where(p => p.Name == Path.GetFileNameWithoutExtension(fileInfo.Name)).FirstOrDefault() != null)
                {
                    // 按照约定，此 html 属于去掉后缀名后的同名文件夹的说明
                    continue;
                }

                var sr = fileInfo.OpenText();
                var htmlContent = sr.ReadToEnd();
                sr.Close();

                var title = Helper.GetHtmlTitle(htmlContent);
                if (string.IsNullOrEmpty(title))
                {
                    title = Path.GetFileNameWithoutExtension(fileInfo.Name);
                }

                var chmItem = new ChmItem
                {
                    Title = title,
                    // HtmlPath = fileInfo.FullName,
                    HtmlPath = fileInfo.FullName.Substring(Config.WorkDirectoryChm.Length).TrimStart('\\'),
                    IsDirectory = false,
                    Items = new List<ChmItem>()
                };
                if (!_readyDefaultChmItem) // 把找到的第一个 html 作为打开 chm 的默认显示页
                {
                    chmItem.IsDefault = true;
                    _readyDefaultChmItem = true;
                }

                chmItemList.Add(chmItem);
            }

            foreach (var dirInfo in dirs)
            {
                var chmItem = new ChmItem
                {
                    Title = Path.GetFileNameWithoutExtension(dirInfo.Name),
                    IsDirectory = true,
                    Items = GetChmItemlList(dirInfo.FullName)
                };

                var htmlFileInfo = files.Where(p => Path.GetFileNameWithoutExtension(p.Name) == dirInfo.Name).FirstOrDefault();
                if (htmlFileInfo != null)
                {
                    // chmItem.HtmlPath = htmlFileInfo.FullName;
                    chmItem.HtmlPath = htmlFileInfo.FullName.Substring(Config.WorkDirectoryChm.Length).TrimStart('\\');
                }

                chmItemList.Add(chmItem);
            }

            return chmItemList;
        }

        private void txtHtmlDirectory_GotFocus(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                txtHtmlDirectory.Text = dialog.FileName;
            }
        }
    }
}
