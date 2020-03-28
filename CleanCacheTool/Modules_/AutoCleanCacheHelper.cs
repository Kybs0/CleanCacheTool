using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CleanCacheTool.Helper;
using CleanCacheTool.Views;
using CleanCacheTool.Views.Models;

namespace CleanCacheTool.Modules_
{
    public class AutoCleanCacheHelper
    {
        DispatcherTimer dispatcherTimer;
        private CleanCacheVeiwModel _viewModel;

        public AutoCleanCacheHelper(CleanCacheVeiwModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Start()
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                //30天清理一次
                dispatcherTimer.Interval = new TimeSpan(20 * 24, 0, 0);
                dispatcherTimer.Tick += DispatcherTimer_Tick;
            }
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            _viewModel.CleanCacheCommand.Execute(null);
            _viewModel.CleanCleanCompleted -= _viewModel_CleanCleanCompleted;
            _viewModel.CleanCleanCompleted += _viewModel_CleanCleanCompleted;
        }

        private void _viewModel_CleanCleanCompleted(object sender, string output)
        {
            _viewModel.CleanCleanCompleted -= _viewModel_CleanCleanCompleted;

            output = DateTime.Now.ToString() + "\r\n" + output + "\r\n";
            var outputFilePath = GetLogPath();
            File.AppendAllText(outputFilePath, output);
        }

        private static string GetLogPath()
        {
            var currentDirectory = CustomPath.GetAppdataFolder();
            var logFolder = Path.Combine(currentDirectory, "Log");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            var iniFilePath = Path.Combine(logFolder, "output.txt");
            if (!File.Exists(iniFilePath))
            {
                var fileStream = File.Create(iniFilePath);
                fileStream.Close();
            }
            return iniFilePath;
        }
    }
}
