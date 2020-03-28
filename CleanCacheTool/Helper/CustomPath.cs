using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCacheTool.Helper
{
    public static class CustomPath
    {
        private static string GetLogErrorPath()
        {
            var currentDirectory = GetAppdataFolder();
            var logFolder = Path.Combine(currentDirectory, "Log");
            EnsureFolderExist(logFolder);
            var iniFilePath = Path.Combine(logFolder, "Error.txt");
            EnsureFileExist(iniFilePath);
            return iniFilePath;
        }

        public static string GetUserSettingIniPath()
        {
            var currentDirectory = GetAppdataFolder();
            var iniFilePath = Path.Combine(currentDirectory, "User.ini");
            EnsureFileExist(iniFilePath);
            return iniFilePath;
        }

        private static void EnsureFolderExist(string logFolder)
        {
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
        }

        private static void EnsureFileExist(string iniFilePath)
        {
            if (!File.Exists(iniFilePath))
            {
                var fileStream = File.Create(iniFilePath);
                fileStream.Close();
            }
        }

        public static string GetAppdataFolder()
        {
            string appdataPath = Path.Combine(@"C:\Users\" + Environment.UserName + @"\AppData\Roaming\CleanCacheTool");
            EnsureFolderExist(appdataPath);
            return appdataPath;
        }
    }
}
