using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHMBuilder.Core
{
    public class ChmItem
    {
        public string Title { get; set; }
        public string HtmlPath { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsDefault { get; set; }
        public List<ChmItem> Items { get; set; }
    }
}
