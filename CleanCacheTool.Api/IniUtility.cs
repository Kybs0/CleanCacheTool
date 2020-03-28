using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CleanCacheTool.Api
{
    public class IniUtility
    {

        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def,
        StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32", EntryPoint = "GetPrivateProfileString")]
        private static extern uint GetPrivateProfileStringA(string section, string key,
            string def, Byte[] retVal, int size, string filePath);

        public static void WriteValue(string section, string key, string value, string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    // 这行代码的作用是创建一个 Unicode 编码的文档
                    File.WriteAllText(path, "", Encoding.Unicode);
                }
            }
            catch (System.Exception err)
            {

            }

            // section=配置节，key=键名，value=键值，path=路径

            WritePrivateProfileString(section, key, value, path);
        }

        public static string ReadValue(string section, string key, string path)
        {
            // 每次从ini中读取多少字节
            var temp = new StringBuilder(2048);

            // section=配置节，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, "", temp, 2048, path);

            return temp.ToString();
        }

        public static List<string> ReadKeys(string sectionName, string iniFilename)
        {
            List<string> result = new List<string>();
            Byte[] buf = new Byte[65536];
            uint len = GetPrivateProfileStringA(sectionName, null, null, buf, buf.Length, iniFilename);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0)
                {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }
        public static string GetKeyValueBySection(string sectionName, string iniFilename)
        {
            var keyValuesBySection = GetKeyValuesBySection(sectionName, iniFilename);

            return keyValuesBySection.Count > 0 ? keyValuesBySection[0] : string.Empty;
        }

        public static List<string> GetKeyValuesBySection(string sectionName, string iniFilename)
        {
            List<string> result = new List<string>();
            var readKeys = ReadKeys(sectionName, iniFilename);
            foreach (var readKey in readKeys)
            {
                var readValue = ReadValue(sectionName, readKey, iniFilename);
                result.Add(readValue);
            }

            return result;
        }

        public static void SaveKeyValuesBySection(IEnumerable<Tuple<string, string>> valueTuples, string iniSection, string iniFilePath)
        {
            foreach (var valueTuple in valueTuples)
            {
                WriteValue(iniSection, valueTuple.Item1, valueTuple.Item2, iniFilePath);
            }
        }

        //清除某个Section
        public static void ClearSection(string iniSection, string iniFilePath)
        {
            try
            {
                WritePrivateProfileString(iniSection, null, null, iniFilePath);
            }
            catch (Exception e)
            {

            }
        }
    }
}
