using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CHMBuilder
{
    class Config
    {
        public static string WorkDirectoryChm = "";
        public static string WorkDirectoryHtml = "";
        public static string WorkDirectoryHtmlSample = "";

        static Config()
        {
            var workDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "WorkDirectory");

            WorkDirectoryChm = workDirectory;

            WorkDirectoryHtml = Path.Combine(workDirectory, "html");
            if (!Directory.Exists(WorkDirectoryHtml))
            {
                Directory.CreateDirectory(WorkDirectoryHtml);
            }

            WorkDirectoryHtmlSample = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "HtmlSample");
        }
    }
}
