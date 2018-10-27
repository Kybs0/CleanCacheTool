using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            p.StandardInput.WriteLine(cmdCommands + "&exit");
            p.StandardInput.AutoFlush = true;

            string strOuput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();

            return strOuput;
        }
    }
}
