using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CleanCacheTool
{
    class ExecuteCmdHelper
    {
        /// <summary>
        /// 执行CMD命令
        /// </summary>
        /// <param name="cmdCommands"></param>
        /// <returns></returns>
        public static string ExecuteCmd(string cmdCommands)
        {
            //创建一个进程
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            process.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.Start();//启动程序

            process.StandardInput.WriteLine(cmdCommands + ExistStr);
            process.StandardInput.AutoFlush = true;
            
            string strOuput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();

            return strOuput;
        }

        public static string ExistStr = "&exit";
    }
}
