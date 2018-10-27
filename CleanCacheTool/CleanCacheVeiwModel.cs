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
            worker.ReportProgress(0);
            _currentOperationDetail = "获取待删除文件..";

            var toCleaningFiles = GetCacheFiles();
            var totalSize = toCleaningFiles.Select(i => new FileInfo(i)).Sum(i => i.Length);
            long handledSize = 0;

            int deleteFilesCount = toCleaningFiles.Count;
            int deleteIndex = 0;
            foreach (var cacheFilePath in toCleaningFiles)
            {
                deleteIndex++;
                //删除文件
                try
                {
                    handledSize += new FileInfo(cacheFilePath).Length;

                    FileUtil.Delete(cacheFilePath);
                    _currentOperationDetail = cacheFilePath;
                }
                catch (Exception exception)
                {
                    _currentOperationDetail = $"Error:{exception.Message}";
                }
                finally
                {
                    _cleanCacheProgressDetail = $"{deleteIndex}/{deleteFilesCount}";
                    //报告进度
                    var progress = totalSize == 0 ? 100 : handledSize * 100 / totalSize;
                    worker.ReportProgress(Convert.ToInt32(progress));
                }
            }

            //删除文件夹下所有的空文件夹
            ToCleanUpManager.Folders.ForEach(folder => FolderUtil.DeleteEmptyFolder(folder, false));
        }

        private void CleanCleanBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Task.Run(() =>
            {
                CleanCacheProgress = e.ProgressPercentage == 0 ? 1 : e.ProgressPercentage;
                OnPropertyChanged(nameof(CleanCacheProgressDetail));
                OnPropertyChanged(nameof(CurrentOperationDetail));

                if (!string.IsNullOrEmpty(CurrentOperationDetail) && CurrentOperationDetail.Contains("Error:"))
                {
                    //ErrorList.Add(_currentOperationDetail.Replace("Error:", string.Empty));
                    var currentText = _currentOperationDetail.Replace("Error:", string.Empty);
                    _errorText += currentText + "\r\n";
                    _errorList.Add(currentText);
                }
            });
        }

        private void CleanCleanBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                CleanCacheProgress = 0;
                IsCleaningCache = false;
                MessageBox.Show($"清理课件缓存时遇到异常！\r\n{e.Error}");
            }
            else
            {
                CleanCacheProgress = 100;
                OnPropertyChanged(nameof(ErrorText));
                OnPropertyChanged(nameof(ErrorList));
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

        private List<string> _errorList = new List<string>() { };
        public List<string> ErrorList
        {
            get => _errorList;
            set
            {
                _errorList = value;
                OnPropertyChanged();
            }
        }

        private string _errorText = string.Empty;

        public string ErrorText
        {
            get => _errorText;
            set
            {
                _errorText = value;
                OnPropertyChanged();
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
                List<string> files = GetCacheFiles();
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

        private List<string> cacheFiles;

        private List<string> GetCacheFiles()
        {
            //if (cacheFiles == null)
            //{
            cacheFiles = new List<string>();

            var folders = ToCleanUpManager.Folders;
            foreach (var folder in folders)
            {
                var allFiles = FolderUtil.GetAllFiles(folder);
                cacheFiles.AddRange(allFiles);
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
