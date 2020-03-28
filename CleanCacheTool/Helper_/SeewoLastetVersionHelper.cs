using System;
using Microsoft.Win32;

namespace CleanCacheTool
{
    public class SeewoLastetVersionHelper
    {
        /// <summary>
        /// 获取版本路径
        /// </summary>
        /// <param name="softwareName">软件名称，如Easinote5</param>
        /// <returns>VersionPath,如C:\Program Files (x86)\Seewo\EasiNote5\EasiNote5_5.1.8.55815</returns>
        public static string GetLastetVersionPath(string softwareName)
        {
            string registData=string.Empty;
            try
            {
                RegistryKey hkml = Registry.LocalMachine;
                RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
                RegistryKey nodeDir = software.OpenSubKey("WOW6432Node", true);
                RegistryKey seewo = nodeDir.OpenSubKey("Seewo", true);
                RegistryKey application = nodeDir.OpenSubKey("Seewo", true);
                registData = application.GetValue("VersionPath").ToString();
            }
            catch (Exception e)
            {
            }
            return registData;
        }
    }
}
