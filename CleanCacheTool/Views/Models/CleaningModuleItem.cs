using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanCacheTool.Api;

namespace CleanCacheTool.Views
{
    /// <summary>
    /// 待清理的模块Item
    /// </summary>
    public class CleaningModuleItem
    {
        public string ModuleFolder { get; set; }

        public long CacheSize { get; set; }

        public string DisplaySize => UnitConverter.ConvertSize(CacheSize);
    }
}
