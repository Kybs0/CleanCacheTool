using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCacheTool.Api
{
    /// <summary>
    /// 硬盘/驱动器辅助类
    /// </summary>
    public class DriveUtil
    {
        /// <summary>
        /// 获取路径所在的驱动器可用空间大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static long GetDriveAvailableSizeByPath(string path)
        {
            DriveInfo driveInfo = new DriveInfo(path);
            return driveInfo.AvailableFreeSpace;
        }
    }
}
