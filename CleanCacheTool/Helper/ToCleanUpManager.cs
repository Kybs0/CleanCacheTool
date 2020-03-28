using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CleanCacheTool.Api;
using CleanCacheTool.Helper;

namespace CleanCacheTool
{
    public class ToCleanUpManager
    {
        private const string IniSection = "清理路径列表（可配置）";
        public static List<string> GetFolders()
        {
            var iniFilePath = CustomPath.GetUserSettingIniPath();

            //获取配置清理路径
            var toCleanUpFolders = IniUtility.GetKeyValuesBySection(IniSection, iniFilePath);
            //重置待清理路径列表
            var allFolders = ResetToCleanFolers(toCleanUpFolders, iniFilePath);
            return allFolders;
        }

        private static List<string> ResetToCleanFolers(List<string> toCleanUpFolders, string iniFilePath)
        {
            var allFolders = new List<string>();
            allFolders.AddRange(toCleanUpFolders.Where(i => Directory.Exists(i)));

            var currentPresetFolders = GetPresetFolders().Where(path => Directory.Exists(path)).ToList();
            if (!currentPresetFolders.All(foler => toCleanUpFolders.Any(i => i == foler)))
            {
                allFolders.AddRange(currentPresetFolders.Where(i => allFolders.All(j => j != i)));
            }

            //重置缓存目录
            IniUtility.ClearSection(IniSection, iniFilePath);
            int index = 0;
            IniUtility.SaveKeyValuesBySection(allFolders.Select(i => Tuple.Create($"路径{index++}", i)), IniSection, iniFilePath);

            return allFolders;
        }

        private static List<string> GetPresetFolders()
        {
            List<string> folders = new List<string>();
            //C盘Windows可清理目录
            folders.AddRange(PresetWindowsFolders);
            //Appdata可清理目录
            folders.AddRange(GetUserDataFolders());
            //Appdata中Seewo可清理目录
            folders.AddRange(GetPresetSeewoDataFolders());
            //安装包版本可清理目录
            folders.AddRange(GetToCleanupVersionFolers());
            return folders;
        }

        private static List<string> PresetWindowsFolders = new List<string>()
        {
            @"C:\Windows\winsxs\Backup",
            //系统更新补丁文件
            @"C:\Windows\SoftwareDistribution\Download",
            @"C:\Windows\Prefetch",
            @"C:\Windows\assembly\temp",
            @"C:\windows\temp",
            @"C:\Windows\Help",
            @"C:\Windows\System32\LogFiles",
        };

        private static List<string> GetUserDataFolders()
        {
            var userFolder = Path.Combine(@"C:\Users\" + Environment.UserName);
            List<string> folders = new List<string>();

            string download = Path.Combine(userFolder, "Downloads");
            folders.Add(download);
            //本地保存临时文件
            string localTemp = Path.Combine(userFolder, @"AppData\Local\Temp");
            folders.Add(localTemp);

            return folders;
        }

        private static List<string> GetPresetSeewoDataFolders()
        {
            string seewoDataPath = Path.Combine(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Seewo");
            string en5DataPath = Path.Combine(seewoDataPath, "EasiNote5");

            List<string> folders = new List<string>();

            string en5Temp = Path.Combine(en5DataPath, "Temp");
            folders.Add(en5Temp);
            string pptServiceTemp = Path.Combine(seewoDataPath, "PPTService", "Temp");
            folders.Add(pptServiceTemp);
            string seewoServiceTemp = Path.Combine(seewoDataPath, "SeewoService", "Temp");
            folders.Add(seewoServiceTemp);
            string seewoAdminTemp = Path.Combine(seewoDataPath, "SeewoAdminService", "Temp");
            folders.Add(seewoAdminTemp);
            string seewoLinkTemp = Path.Combine(seewoDataPath, "SeewoLink", "Temp");
            folders.Add(seewoLinkTemp);
            return folders;
        }

        public static List<string> GetCleanCacheCommands()
        {
            List<string> commands = new List<string>
            {
                //删除DNS缓存
                "ipconfig/flushDNS",
                @"RmDir /q /s C:\Windows\Installer\$PatchCache$",
            };

            return commands;
        }

        /// <summary>
        /// 超长时间
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCleanDelayedCommands()
        {
            List<string> commands = new List<string>
            {
                //删除windows更新缓存--更新时间太长，放弃
                "Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase",
            };

            return commands;
        }

        private static List<string> GetToCleanupVersionFolers()
        {
            var folders = new List<string>();
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            if (programFiles != null)
            {
                string seewoInstallFolder = Path.Combine(programFiles, "Seewo");

                var easiCare = Path.Combine(seewoInstallFolder, "EasiCare");
                var easiCareToCleanupFolers = GetToCleanupFolers(easiCare);
                folders.AddRange(easiCareToCleanupFolers);

                var easiCamera = Path.Combine(seewoInstallFolder, "EasiCamera");
                var easiCameraToCleanupFolers = GetToCleanupFolers(easiCamera);
                folders.AddRange(easiCameraToCleanupFolers);

                var pptChecker = Path.Combine(seewoInstallFolder, "PPTChecker");
                var pptCheckerToCleanupFolers = GetToCleanupFolers(pptChecker);
                folders.AddRange(pptCheckerToCleanupFolers);


                var seewoLink = Path.Combine(seewoInstallFolder, "SeewoLink");
                var seewoLinkToCleanupFolers = GetToCleanupFolers(seewoLink);
                folders.AddRange(seewoLinkToCleanupFolers);
            }

            return folders;
        }

        private static List<string> GetToCleanupFolers(string softwareFolder)
        {
            if (!Directory.Exists(softwareFolder))
            {
                return new List<string>();
            }

            var versionRegex = new Regex(@"^.*_\d+(\.\d+)*$");
            var versionFolders = new DirectoryInfo(softwareFolder).GetDirectories().Where(foler => versionRegex.IsMatch(foler.Name)).Select(i => i.FullName).ToList();

            string lastetVersionOrPath = string.Empty;
            lastetVersionOrPath = SeewoLastetVersionHelper.GetLastetVersionPathByIni(softwareFolder);
            if (string.IsNullOrEmpty(lastetVersionOrPath))
            {
                lastetVersionOrPath = SeewoLastetVersionHelper.GetLastetVersionPathByName(Path.GetFileNameWithoutExtension(softwareFolder));
            }

            var toCleanFolers = versionFolders.Where(i => !i.Contains(lastetVersionOrPath)).ToList();

            return toCleanFolers;
        }
    }
}
