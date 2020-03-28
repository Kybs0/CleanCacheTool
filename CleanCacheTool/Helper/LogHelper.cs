using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CleanCacheTool.Helper
{
    public static class LogHelper
    {
        public static void Error(string error, [CallerMemberName] string propertyName = null)
        {
            var logErrorPath = GetLogErrorPath();
            File.AppendAllText(logErrorPath, $"Method:{propertyName},{error}\r\n");
        }
        private static string GetLogErrorPath()
        {
            var currentDirectory = CustomPath.GetAppdataFolder();
            var logFolder = Path.Combine(currentDirectory, "Log");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            var iniFilePath = Path.Combine(logFolder, "Error.txt");
            if (!File.Exists(iniFilePath))
            {
                var fileStream = File.Create(iniFilePath);
                fileStream.Close();
            }
            return iniFilePath;
        }
    }
}
