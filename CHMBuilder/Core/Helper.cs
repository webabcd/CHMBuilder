using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CHMBuilder.Core
{
    public class Helper
    {
        public static void SaveFile(string filePath, string fileContent)
        {
            StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
            sw.Write(fileContent);
            sw.Close();
        }

        public static void SaveFileGBK(string filePath, string fileContent)
        {
            // GB18030 就是 GBK，因为微软的 chm 生成工具不支持 utf-8，所以生成 hhp, hhc, hhk 文件时需要用此编码
            StreamWriter sw = new StreamWriter(filePath, false, Encoding.GetEncoding("GB18030"));
            sw.Write(fileContent);
            sw.Close();
        }

        public static string GetHtmlTitle(string htmlContent)
        {
            Regex reg = new Regex(@"(?m)<title[^>]*>(?<title>(?:\w|\W)*?)</title[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match mc = reg.Match(htmlContent);
            if (mc.Success)
            {
                return mc.Groups["title"].Value.Trim();
            }
            else
            {
                return "";
            }
        }

        public static void CopyDirectory(string sourcePath, string destPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, destPath), true);
            }
        }
    }
}
