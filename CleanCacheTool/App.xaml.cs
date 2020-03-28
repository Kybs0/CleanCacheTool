using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using CleanCacheTool.Modules_;
using CleanCacheTool.Views.Models;
using Microsoft.Win32;

namespace CleanCacheTool
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            KillProcess(System.Windows.Forms.Application.ProductName);
            //SetAppAutoRun(false);
        }

        /// <summary>
        /// 删除原有进程
        /// </summary>
        /// <param name="processName"></param>
        private void KillProcess(string processName)
        {
            try
            {
                //删除所有同名进程
                Process currentProcess = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(processName).Where(process => process.Id != currentProcess.Id);
                foreach (Process thisproc in processes)
                {

                    thisproc.Kill();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void SetAppAutoRun(bool autoRun)
        {
            if (autoRun) //设置开机自启动  
            {
                string path = System.Windows.Forms.Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue("JcShutdown", path);
                rk2.Close();
                rk.Close();
            }
            else //取消开机自启动  
            {
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue("JcShutdown", false);
                rk2.Close();
                rk.Close();
            }
        }
    }
}
