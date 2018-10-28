using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CleanCacheTool.Annotations;

namespace CleanCacheTool
{
    public class CleanCacheVeiwModel : INotifyPropertyChanged
    {
        private static CleanCacheVeiwModel _viewModel;

        public static CleanCacheVeiwModel ViewModel => _viewModel ?? (_viewModel = new CleanCacheVeiwModel());

        public CleanCacheVeiwModel()
        {
            Task.Run(() =>
            {
                RefreshCacheSize();
            });

            RefreshCommand = new ActionCommand(RefreshCacheSize);
            CleanCacheCommand = new ActionCommand(CleanCacheWithWorker);
        }

        #region 清理缓存

        public ICommand CleanCacheCommand { get; }

        private BackgroundWorker _currentWorker = null;
        private void CleanCacheWithWorker()
        {
            _currentWorker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            _currentWorker.ProgressChanged += CleanCleanBackgroundWorker_ProgressChanged;
            _currentWorker.DoWork += CleanCleanBackgroundWorker_DoWork;
            _currentWorker.RunWorkerCompleted += CleanCleanBackgroundWorker_RunWorkerCompleted;
            _currentWorker.RunWorkerAsync();
        }

        private void CleanCleanBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            IsCleaningCache = true;
            ErrorText = string.Empty;
            worker.ReportProgress(0);

            //使用CMD清理Winsxc
            CacheCacheByCmd(worker);
            //删除文件
            CleanCacheUsingFileUtil(worker);

            //删除文件夹下所有的空文件夹
            ToCleanUpManager.Folders.ForEach(folder => FolderUtil.DeleteEmptyFolder(folder, false));
        }

        private void CacheCacheByCmd(BackgroundWorker worker)
        {
            _currentOperationError = string.Empty;
            worker.ReportProgress(0);
            var commands = ToCleanUpManager.GetCleanCacheCommands();

            int index = 0;
            foreach (var command in commands)
            {
                index++;

                try
                {
                    _currentOperationDetail = $"执行{command}";
                    ExecuteCmdHelper.ExecuteCmd(command);
                }
                catch (Exception e)
                {
                    _currentOperationError = $"{command}执行失败";
                }
                finally
                {
                    worker.ReportProgress(Convert.ToInt32(Convert.ToDouble(index) / Convert.ToDouble(commands.Count)));
                }
            }
        }

        private void CleanCacheUsingFileUtil(BackgroundWorker worker)
        {
            _currentOperationError = string.Empty;
            //1.获取待删除文件
            _currentOperationDetail = "获取待删除文件..";
            worker.ReportProgress(0);

            var toCleaningDictionary = GetCacheFiles();
            List<string> toCleaningFiles = new List<string>();
            foreach (var keyValuePair in GetCacheFiles())
            {
                toCleaningFiles.AddRange(keyValuePair.Value);
            }
            var totalSize = toCleaningFiles.Select(i => new FileInfo(i)).Sum(i => i.Length);
            long handledSize = 0;

            //2.使用管理员权限删除文件
            int deleteFilesCount = toCleaningFiles.Count;
            int deleteIndex = 0;
            foreach (var toCleaningKeypair in toCleaningDictionary)
            {
                _currentOperationDetail = $"删除{toCleaningKeypair.Key}";
                foreach (var cacheFilePath in toCleaningKeypair.Value)
                {
                    deleteIndex++;

                    try
                    {
                        handledSize += new FileInfo(cacheFilePath).Length;

                        FileUtil.Delete(cacheFilePath);
                    }
                    catch (Exception exception)
                    {
                        _currentOperationError = exception.Message;
                    }
                    finally
                    {
                        _cleanCacheProgressDetail = $"{deleteIndex}/{deleteFilesCount}";
                        //报告进度
                        var progress = totalSize == 0 ? 100 : handledSize * 100 / totalSize;
                        worker.ReportProgress(Convert.ToInt32(progress));
                    }
                }
            }
        }

        private void CleanCleanBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Task.Run(() =>
            {
                CleanCacheProgress = e.ProgressPercentage == 0 ? 1 : e.ProgressPercentage;
                OnPropertyChanged(nameof(CleanCacheProgressDetail));
                OnPropertyChanged(nameof(CurrentOperationDetail));

                if (!string.IsNullOrEmpty(_currentOperationError))
                {
                    _errorText += _currentOperationError + "\r\n";
                }
            });
        }

        private void CleanCleanBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CleanCacheProgress = 0;
                IsCleaningCache = false;
                _currentOperationError = string.Empty;
                MessageBox.Show($"清理课件缓存时遇到异常！\r\n{e.Error}");
            }
            else
            {
                CleanCacheProgress = 100;
                _currentOperationError = string.Empty;
                OnPropertyChanged(nameof(ErrorText));
                OnPropertyChanged(nameof(ErrorListCount));

                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    CacheSize = UnitConverter.ConvertSize(0);
                    CleanCacheProgress = 0;
                    IsCleaningCache = false;
                });
            }
            _currentWorker.Dispose();
        }

        /// <summary>
        /// 操作异常
        /// </summary>
        private string _currentOperationError = string.Empty;

        private string _currentOperationDetail = string.Empty;

        public string CurrentOperationDetail
        {
            get => _currentOperationDetail;
            set
            {
                _currentOperationDetail = value;
                OnPropertyChanged();
            }
        }

        public int ErrorListCount => Regex.Matches(ErrorText, "\r\n").Count;

        private string _errorText = string.Empty;

        public string ErrorText
        {
            get => _errorText;
            set
            {
                _errorText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ErrorListCount));
            }
        }

        private int _cleanCacheProgress;
        public int CleanCacheProgress
        {
            get => _cleanCacheProgress;
            set
            {
                _cleanCacheProgress = value;
                OnPropertyChanged();
            }
        }

        private string _cleanCacheProgressDetail;
        public string CleanCacheProgressDetail
        {
            get => _cleanCacheProgressDetail;
            set
            {
                _cleanCacheProgressDetail = value;
                OnPropertyChanged();
            }
        }

        private bool _isCleaningCache = false;
        public bool IsCleaningCache
        {
            get => _isCleaningCache;
            set
            {
                _isCleaningCache = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region 缓存显示

        public ICommand RefreshCommand { get; }

        private void RefreshCacheSize()
        {
            CacheSize = "获取中...";
            //课件缓存显示
            Task.Run(() =>
            {
                List<string> files = new List<string>();
                foreach (var keyValuePair in GetCacheFiles())
                {
                    files.AddRange(keyValuePair.Value);
                }
                var cacheSpaceSize = files.Select(i => new FileInfo(i)).Sum(fileInfo => fileInfo.Length);

                CacheSize = UnitConverter.ConvertSize(cacheSpaceSize);
            });
        }

        private string _cacheSize = "获取中...";

        /// <summary>
        /// 待清理空间大小
        /// </summary>
        public string CacheSize
        {
            get => _cacheSize;
            set
            {
                _cacheSize = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region 缓存列表

        private Dictionary<string, List<string>> cacheFiles;

        /// <summary>
        /// 获取缓存列表
        /// </summary>
        /// <returns>文件夹，文件列表</returns>
        private Dictionary<string, List<string>> GetCacheFiles()
        {
            //if (cacheFiles == null)
            //{
            cacheFiles = new Dictionary<string, List<string>>();

            var folders = ToCleanUpManager.Folders;
            foreach (var folder in folders)
            {
                var allFiles = FolderUtil.GetAllFiles(folder);
                cacheFiles.Add(folder, allFiles);
            }
            //}

            return cacheFiles;
        }

        #endregion

        #region 辅助方法

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    internal class CleanCacheBackgroundWorkerModel
    {
        public CleanCacheBackgroundWorkerModel()
        {
        }

        public List<string> ToCleaningFiles { get; set; }
    }
}
