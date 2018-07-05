using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CHMBuilder.Core
{
    public class ChmHelper
    {
        private string _chmTitle;
        private List<ChmItem> _chmItemList;
        private ChmItem _defaultChmItem;

        public ChmHelper(string chmTitle, List<ChmItem> chmItemList)
        {
            _chmTitle = chmTitle;
            _chmItemList = chmItemList;
            _defaultChmItem = GetDefaultChmItem(chmItemList);
        }

        public string SaveChm()
        {
            SaveHhp();
            SaveHhc();
            SaveHhk();

            var workDirectory = Config.WorkDirectoryChm;
            var filePathChm = Path.Combine(workDirectory, _chmTitle + ".chm");
            var filePathHhp = Path.Combine(workDirectory, "hhp.hhp");
            var filePathHhcExe = Path.Combine(workDirectory, "hhc.exe");

            Process process = new Process();
            try
            {
                if (File.Exists(filePathChm))
                {
                    File.Delete(filePathChm);
                }

                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.FileName = filePathHhcExe;  // hhc.exe 文件的路径
                process.StartInfo.Arguments = "\"" + filePathHhp + "\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                Helper.SaveFile(Path.Combine(workDirectory, "process.log"), output);

                process.WaitForExit();

                if (process.ExitCode == 0) // 编译过程中遇到了一些警告（仍然可以编译出 chm）的话 hhc.ext 会返回 0，编译完全正常的话会返回 1
                {

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                process.Close();
            }

            return filePathChm;
        }

        // 对应 chm 的编译
        private void SaveHhp()
        {
            var workDirectory = Config.WorkDirectoryChm;
            var filePath = Path.Combine(workDirectory, "hhp.hhp");

            var content = new StringBuilder();

            content.AppendLine("[OPTIONS]");
            content.AppendLine("Title=" + _chmTitle);
            content.AppendLine("Compatibility=1.1 or later");
            content.AppendLine("Compiled file=" + _chmTitle + ".chm");
            content.AppendLine("Contents file=hhc.hhc");
            content.AppendLine("Index file=hhk.hhk");
            if (_defaultChmItem != null)
            {
                content.AppendLine("Default topic=" + _defaultChmItem.HtmlPath);
            }
            content.AppendLine("Display compile progress=NO");
            content.AppendLine("Full-text search=Yes");
            content.AppendLine("Language=0x804 中文(简体，中国)");
            content.AppendLine();
            content.AppendLine("[FILES]");
            content.AppendLine();
            content.AppendLine("[INFOTYPES]");
            content.AppendLine();

            Helper.SaveFileGBK(filePath, content.ToString());
        }

        // 对应 chm 的目录
        private void SaveHhc()
        {
            var workDirectory = Config.WorkDirectoryChm;
            var filePath = Path.Combine(workDirectory, "hhc.hhc");

            var content = new StringBuilder();

            content.AppendLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
            content.AppendLine("<HTML>");
            content.AppendLine("<HEAD>");
            content.AppendLine("<meta name=\"GENERATOR\" content=\"Microsoft&reg; HTML Help Workshop 4.1\">");
            content.AppendLine("<!-- Sitemap 1.0 -->");
            content.AppendLine("</HEAD>");
            content.AppendLine("<BODY>");
            content.AppendLine("<OBJECT type=\"text/site properties\">");
            content.AppendLine("<param name=\"Window Styles\" value=\"0x237\">");
            content.AppendLine("</OBJECT>");

            content.AppendLine(GetHhcCoreContent(_chmItemList));

            content.AppendLine("</BODY>");
            content.AppendLine("</HTML>");
            content.AppendLine();

            Helper.SaveFileGBK(filePath, content.ToString());
        }
        private string GetHhcCoreContent(List<ChmItem> chmItemList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var chmItem in chmItemList)
            {
                sb.AppendLine("<UL>");
                sb.AppendLine("<LI> <OBJECT type=\"text/sitemap\">");
                sb.AppendLine("<param name=\"Name\" value=\"" + chmItem.Title + "\">");
                if (!string.IsNullOrEmpty(chmItem.HtmlPath))
                {
                    sb.AppendLine("<param name=\"Local\" value=\"" + chmItem.HtmlPath + "\">");
                }
                sb.AppendLine("</OBJECT>");
                if (chmItem.IsDirectory)
                {
                    sb.AppendLine(GetHhcCoreContent(chmItem.Items));
                }
                sb.AppendLine("</UL>");
            }

            return sb.ToString();
        }

        // 对应 chm 的索引
        private void SaveHhk()
        {
            var workDirectory = Config.WorkDirectoryChm;
            var filePath = Path.Combine(workDirectory, "hhk.hhk");

            var content = new StringBuilder();

            content.AppendLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
            content.AppendLine("<HTML>");
            content.AppendLine("<HEAD>");
            content.AppendLine("<meta name=\"GENERATOR\" content=\"Microsoft&reg; HTML Help Workshop 4.1\">");
            content.AppendLine("<!-- Sitemap 1.0 -->");
            content.AppendLine("</HEAD>");
            content.AppendLine("<BODY>");
            content.AppendLine("<UL>");

            content.AppendLine(GetHhkCoreContent(_chmItemList));

            content.AppendLine("</UL>");
            content.AppendLine("</BODY>");
            content.AppendLine("</HTML>");
            content.AppendLine();

            Helper.SaveFileGBK(filePath, content.ToString());
        }
        private string GetHhkCoreContent(List<ChmItem> chmItemList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var chmItem in chmItemList)
            {
                if (!string.IsNullOrEmpty(chmItem.HtmlPath))
                {
                    sb.AppendLine("<LI> <OBJECT type=\"text/sitemap\">");
                    sb.AppendLine("<param name=\"Name\" value=\"" + chmItem.Title + "\">");
                    sb.AppendLine("<param name=\"Local\" value=\"" + chmItem.HtmlPath + "\">");
                    sb.AppendLine("</OBJECT>");
                }

                if (chmItem.IsDirectory)
                {
                    sb.AppendLine(GetHhkCoreContent(chmItem.Items));
                }
            }

            return sb.ToString();
        }

        private ChmItem GetDefaultChmItem(List<ChmItem> chmItemList)
        {
            foreach (var chmItem in chmItemList)
            {
                if (chmItem.IsDefault)
                {
                    return chmItem;
                }
                else
                {
                    var defaultChmItem = GetDefaultChmItem(chmItem.Items);
                    if (defaultChmItem != null)
                    {
                        return defaultChmItem;
                    }
                }
            }

            return null;
        }
    }
}
