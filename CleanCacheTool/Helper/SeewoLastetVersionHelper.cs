using System;
using System.IO;
using CleanCacheTool.Api;
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
        public static string GetLastetVersionPathByName(string softwareName)
        {
            string registData = string.Empty;
            try
            {
                RegistryKey hkml = Registry.LocalMachine;
                RegistryKey software = hkml.OpenSubKey("SOFTWARE", false);
                RegistryKey seewo = software.OpenSubKey("Seewo", false);
                RegistryKey application = seewo.OpenSubKey(softwareName, false);
                registData = application.GetValue("VersionPath").ToString();
            }
            catch (Exception e)
            {
            }
            return registData;
        }

        /// <summary>
        /// 通过软件安装路径中的launcherConfig.ini获取当前最新版本路径
        /// </summary>
        /// <param name="softwareFolder"></param>
        /// <returns></returns>
        public static string GetLastetVersionPathByIni(string softwareFolder)
        {
            string versionPath = string.Empty;
            var iniPath = Path.Combine(softwareFolder, "swenlauncher\\launcherConfig.ini");
            if (File.Exists(versionPath))
            {
                try
                {
                    var actualExePath = IniUtility.ReadValue("Version", "ActualExePath", iniPath);
                    var leftPathStr = actualExePath.Replace(softwareFolder + "\\", string.Empty);
                    var versionFileName = leftPathStr.Substring(0, leftPathStr.IndexOf("\\"));
                    versionPath = Path.Combine(softwareFolder, versionFileName);
                }
                catch (Exception e)
                {
                }
            }

            return versionPath;
        }
    }
}
