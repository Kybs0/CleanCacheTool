using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CleanCacheTool
{
    public class ToCleanUpManager
    {
        private const string IniSection = "可清理路径列表";
        public static List<string> GetFolders()
        {
            var iniFilePath = GetIniFilePath();
            var toCleanUpFolders = IniUtility.GetKeyValuesBySection(IniSection, iniFilePath);
            return toCleanUpFolders;
        }

        private static string GetIniFilePath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var iniFilePath = Path.Combine(currentDirectory, "User.ini");
            if (!File.Exists(iniFilePath))
            {
                var fileStream = File.Create(iniFilePath);
                fileStream.Close();

                var presetFolders = GetPresetFolders().Where(path => Directory.Exists(path)).ToList();
                IniUtility.SaveKeyValuesBySection(
                    presetFolders.Select(i => Tuple.Create(Path.GetFileNameWithoutExtension(i), i)), IniSection,
                    iniFilePath);
            }
            return iniFilePath;
        }

        public static List<string> Folders => GetFolders();

        private static List<string> GetPresetFolders()
        {
            List<string> folders = new List<string>();

            folders.AddRange(PresetWindowsFolders);
            folders.AddRange(GetPresetSeewoDataFolders());
            return folders;
        }

        private static List<string> PresetWindowsFolders = new List<string>()
        {
            @"C:\Windows\winsxs\Backup",
            @"C:\Windows\Installer\$PatchCache$\Managed",
            @"C:\Windows\SoftwareDistribution\Download",
            @"C:\Windows\Prefetch",
            @"C:\Users\10167\AppData\Local\Temp",
            @"C:\Windows\assembly\temp",
        };

        private static List<string> GetPresetSeewoDataFolders()
        {
            string seewoDataPath = Path.Combine(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Seewo");
            string en5DataPath = Path.Combine(seewoDataPath, "EasiNote5");

            List<string> folders = new List<string>();

            string en5Temp = Path.Combine(en5DataPath, "Temp");
            folders.Add(en5Temp);
            string en5Dependencies = Path.Combine(en5DataPath, "Dependencies");
            folders.Add(en5Dependencies);

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
                //删除windows更新缓存
                "Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase",
                //删除DNS缓存
                "ipconfig/flushDNS"
            };

            return commands;
        }
    }
}
