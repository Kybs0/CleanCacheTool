using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CleanCacheTool.Annotations;
using CleanCacheTool.Api;
using CleanCacheTool.Controls;
using CleanCacheTool.Helper;

namespace CleanCacheTool.Views.Models
{
    public class CleanCacheVeiwModel : NotifyPropertyChanged
    {
        private static CleanCacheVeiwModel _cleanCacheVeiwModel = null;

        public static CleanCacheVeiwModel Instance =>
            _cleanCacheVeiwModel ?? (_cleanCacheVeiwModel = new CleanCacheVeiwModel());
        private CleanCacheVeiwModel()
        {
            RefreshCommand = new ActionCommand(Refresh);
            CleanCacheByMainViewCommand = new ActionCommand(CleanCacheByMainView);
            CleanCacheCommand = new ActionCommand(CleanCacheWithWorker);
            DocListState = EditStatus.Editing;
        }

        private void CleanCacheByMainView()
        {
            IsCleaningCache = true;
            Task.Run(() =>
            {
                if (ItemsSource == null || ItemsSource.Count == 0)
                {
                    var cleaningModuleItems = GetCleaningUpFiles(out long size);
                    ItemsSource = cleaningModuleItems;
                }

                if (SelectedItems == null || SelectedItems.Count == 0)
                {
                    SelectedItems = ItemsSource;
                }

                CleanCacheWithWorker();
            });
        }

        #region 清理缓存

        public ICommand CleanCacheByMainViewCommand { get; }
        public ICommand CleanCacheCommand { get; }

        private BackgroundWorker _currentWorker = null;
        private void CleanCacheWithWorker()
        {
            if (_currentWorker != null && _currentWorker.IsBusy || SelectedItems == null || SelectedItems.Count == 0)
            {
                return;
            }
            _currentWorker?.Dispose();
            _currentWorker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            _currentWorker.ProgressChanged += CleanCleanBackgroundWorker_ProgressChanged;
            _currentWorker.DoWork += CleanCleanBackgroundWorker_DoWork;
            _currentWorker.RunWorkerCompleted += CleanCleanBackgroundWorker_RunWorkerCompleted;
            _currentWorker.RunWorkerAsync();
            ExecuteDelayCommands();
        }

        /// <summary>
        /// 时间较长，额外处理
        /// </summary>
        private async void ExecuteDelayCommands()
        {
            var cleanDelayedCommands = ToCleanUpManager.GetCleanDelayedCommands();
            foreach (var cleanDelayedCommand in cleanDelayedCommands)
            {
                await Task.Run(() =>
                    {
                        ExecuteCmdHelper.ExecuteCmd(cleanDelayedCommand);
                    });
                OutputText += $"删除命令{cleanDelayedCommand}\r\n";
            }
        }

        private void CleanCleanBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            IsCleaningCache = true;
            OutputText = string.Empty;
            ErrorText = string.Empty;
            worker.ReportProgress(0);

            //使用CMD清理Winsxc等
            //CacheCacheByCmd(worker);
            //删除文件
            var toCleanFolders = SelectedItems.Select(I => I.ModuleFolder).ToList();
            Application.Current.Dispatcher.Invoke(() => { ItemsSource.Clear(); });
            CleanCacheUsingFileUtil(worker, toCleanFolders);

            //删除文件夹下所有的空文件夹
            foreach (var folder in ToCleanUpManager.GetFolders())
            {
                try
                {
                    FolderUtil.DeleteEmptyFolder(folder, false);
                }
                catch (Exception exception)
                {
                    LogHelper.Error(exception.Message);
                }
            }
        }

        /// <summary>
        /// 执行Cmd命令清理缓存
        /// </summary>
        /// <param name="worker"></param>
        private void CacheCacheByCmd(BackgroundWorker worker)
        {
            worker.ReportProgress(0);
            var commands = ToCleanUpManager.GetCleanCacheCommands();

            int index = 0;
            foreach (var command in commands)
            {
                index++;
                var currentOperationError = string.Empty;
                var currentOperationOutput = string.Empty;
                try
                {
                    var currentOperationDetail = $"执行{command}，可能需要几分钟，请等待";
                    worker.ReportProgress(-1,
                        new ProgressChangedContent()
                        {
                            OperationDetail = currentOperationDetail,
                        });
                    if (command.Contains(@"C:\Windows\Installer\$PatchCache$"))
                    {
                        ExecuteCmdHelper.ExecuteCmd("Net Start msiserver /Y");
                        ExecuteCmdHelper.ExecuteCmd("Net Stop msiserver /Y");
                        ExecuteCmdHelper.ExecuteCmd(@"Reg Add HKLM\Software\Policies\Microsoft\Windows\Installer /v MaxPatchCacheSize /t REG_DWORD /d 0 /f");
                        ExecuteCmdHelper.ExecuteCmd(command);
                        ExecuteCmdHelper.ExecuteCmd("Net Start msiserver /Y");
                        ExecuteCmdHelper.ExecuteCmd("Net Stop msiserver /Y");
                        ExecuteCmdHelper.ExecuteCmd(@"Reg Add HKLM\Software\Policies\Microsoft\Windows\Installer /v MaxPatchCacheSize /t REG_DWORD /d 10 /f");
                        ExecuteCmdHelper.ExecuteCmd("Net Start msiserver /Y");
                    }
                    var executeCmdResult = ExecuteCmdHelper.ExecuteCmd(command);
                    currentOperationOutput += $"命令{command}";
                }
                catch (Exception e)
                {
                    LogHelper.Error(e.Message);
                    currentOperationError = $"{command}执行失败";
                }
                finally
                {
                    worker.ReportProgress(Convert.ToInt32(Convert.ToDouble(index) / Convert.ToDouble(commands.Count)),
                        new ProgressChangedContent()
                        {
                            OperationOutput = currentOperationOutput,
                            OperationError = currentOperationError
                        });
                }
            }
        }

        private long GetCurrentCacheSize()
        {
            List<string> toCleaningFiles = new List<string>();
            long totalSize = 0;

            foreach (var keyValuePair in GetCacheFiles())
            {
                toCleaningFiles.AddRange(keyValuePair.Value);
            }

            foreach (var cleaningFile in toCleaningFiles)
            {
                var length = FileUtil.GetFileSize(cleaningFile);
                totalSize += length;
            }

            return totalSize;
        }

        /// <summary>
        /// 通过文件清理缓存
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="folders"></param>
        private void CleanCacheUsingFileUtil(BackgroundWorker worker, List<string> folders)
        {
            //1.获取待删除文件
            var currentOperationDetail = "获取待删除文件..";
            worker.ReportProgress(0, new ProgressChangedContent()
            {
                OperationDetail = currentOperationDetail
            });

            var toCleaningDictionary = GetCacheFiles(folders);
            List<string> toCleaningFiles = new List<string>();
            foreach (var keyValuePair in toCleaningDictionary)
            {
                toCleaningFiles.AddRange(keyValuePair.Value);
            }
            var totalSize = GetCurrentCacheSize();
            long handledSize = 0;

            //2.使用管理员权限删除文件
            int deleteFilesCount = toCleaningFiles.Count;
            int deleteIndex = 0;
            foreach (var toCleaningKeypair in toCleaningDictionary)
            {
                currentOperationDetail = $"正在删除{toCleaningKeypair.Key}";
                worker.ReportProgress(-1, new ProgressChangedContent()
                {
                    OperationDetail = currentOperationDetail,
                });
                long currentFolderCleanedSize = 0;
                var currentOperationError = string.Empty;

                foreach (var cacheFilePath in toCleaningKeypair.Value)
                {
                    deleteIndex++;
                    try
                    {
                        var cacheFileSize = new FileInfo(cacheFilePath).Length;
                        handledSize += cacheFileSize;

                        FileUtil.Delete(cacheFilePath);

                        currentFolderCleanedSize += cacheFileSize;
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e.Message);
                        currentOperationError = e.Message;
                    }
                    finally
                    {
                        var cleanCacheProgressDetail = $"{deleteIndex}/{deleteFilesCount}";
                        //报告进度
                        var progress = totalSize == 0 ? 100 : handledSize * 100 / totalSize;
                        worker.ReportProgress(Convert.ToInt32(progress), new ProgressChangedContent()
                        {
                            ProgressDetail = cleanCacheProgressDetail,
                            OperationError = currentOperationError
                        });
                    }
                }

                worker.ReportProgress(-1, new ProgressChangedContent()
                {
                    OperationOutput = $"{toCleaningKeypair.Key}已删除{UnitConverter.ConvertSize(currentFolderCleanedSize)}"
                });
            }
        }

        private void CleanCleanBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0)
            {
                CleanCacheProgress = e.ProgressPercentage == 0 ? 1 : e.ProgressPercentage;
            }
            if (e.UserState is ProgressChangedContent progressChangedContent)
            {
                if (!string.IsNullOrEmpty(progressChangedContent.ProgressDetail))
                {
                    CleanCacheProgressDetail = progressChangedContent.ProgressDetail;
                }
                if (!string.IsNullOrEmpty(progressChangedContent.OperationDetail))
                {
                    CurrentOperationDetail = progressChangedContent.OperationDetail;
                }
                if (!string.IsNullOrEmpty(progressChangedContent.OperationOutput))
                {
                    OutputText += progressChangedContent.OperationOutput + "\r\n";
                }
                if (!string.IsNullOrEmpty(progressChangedContent.OperationError))
                {
                    _errorText += progressChangedContent.OperationError + "\r\n";
                }
            }

        }

        public event EventHandler<string> CleanCleanCompleted;
        private void CleanCleanBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var completedOutput = OutputText;
            if (e.Error != null)
            {
                CleanCacheProgress = 0;
                IsCleaningCache = false;
                completedOutput += ($"清理缓存时遇到异常！\r\n{e.Error}");
            }
            else
            {
                CleanCacheProgress = 100;
                OnPropertyChanged(nameof(OutputText));
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
            //刷新列表
            Refresh();

            CleanCleanCompleted?.Invoke(null, completedOutput);
            _currentWorker.Dispose();
        }

        #region 操作进度与异常

        private string _currentOperationDetail = string.Empty;

        /// <summary>
        /// 操作进度
        /// </summary>
        public string CurrentOperationDetail
        {
            get => _currentOperationDetail;
            set
            {
                _currentOperationDetail = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region 清理进度显示

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

        #endregion

        #region 输出日志

        private string _outputText = string.Empty;

        public string OutputText
        {
            get => _outputText;
            set
            {
                _outputText = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region 错误列表

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

        #endregion

        #endregion

        #region 刷新

        public ICommand RefreshCommand { get; }
        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        private void Refresh()
        {
            Task.Run(async () =>
                {
                    try
                    {
                        if (!IsRefreshing)
                        {
                            IsRefreshing = true;

                            ItemsSource = new List<CleaningModuleItem>();

                            await Task.Delay(TimeSpan.FromSeconds(0.5));

                            var cleaningFileItems = GetCleaningUpFiles(out long totalSize);
                            CacheSize = UnitConverter.ConvertSize(totalSize);
                            ItemsSource = cleaningFileItems;

                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e.Message);
                        MessageBox.Show(e.Message);
                    }
                    IsRefreshing = false;
                });
        }
        public List<CleaningModuleItem> GetCleaningUpFiles(out long totalSize)
        {
            List<CleaningModuleItem> cacheFiles = new List<CleaningModuleItem>();
            var keyValuePairs = GetCacheFiles();

            totalSize = 0;
            foreach (var keyValuePair in keyValuePairs)
            {
                var subModuleSize = keyValuePair.Value.Select(TyGetFileLength).ToList().Sum();
                if (subModuleSize > 0)
                {
                    cacheFiles.Add(new CleaningModuleItem()
                    {
                        ModuleFolder = keyValuePair.Key,
                        CacheSize = subModuleSize
                    });
                }

                totalSize += subModuleSize;
            }
            return cacheFiles;
        }

        private long TyGetFileLength(string cleaningFile)
        {
            try
            {
                var length = new FileInfo(cleaningFile).Length;
                return length;
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message);
            }

            return 0;
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

        /// <summary>
        /// 获取缓存列表
        /// </summary>
        /// <returns>文件夹，文件列表</returns>
        private Dictionary<string, List<string>> GetCacheFiles(List<string> folders)
        {
            Dictionary<string, List<string>> cacheFiles = new Dictionary<string, List<string>>();

            foreach (var folder in folders)
            {
                var allFiles = FolderUtil.GetAllFiles(folder);
                //筛选出>1M的文件
                allFiles = allFiles.Where(i => FileUtil.GetFileSize(i) > 1048576).ToList();
                if (allFiles.Count > 0)
                {
                    cacheFiles.Add(folder, allFiles);
                }
            }

            return cacheFiles;
        }

        /// <summary>
        /// 获取缓存列表
        /// </summary>
        /// <returns>文件夹，文件列表</returns>
        private Dictionary<string, List<string>> GetCacheFiles()
        {
            var folders = ToCleanUpManager.GetFolders();
            Dictionary<string, List<string>> cacheFiles = GetCacheFiles(folders);
            return cacheFiles;
        }

        #endregion

        #region 属性

        /// <summary>
        /// 用于绑定界面的所有列表项，包括待接收的分享来的课件
        /// </summary>
        public List<CleaningModuleItem> ItemsSource
        {
            get => _itemsSource;
            set
            {
                var items = value;
                _itemsSource = items;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 课件列表的状态
        /// Editing：编辑模式
        /// </summary>
        public EditStatus DocListState
        {
            get => _docListState;
            set
            {
                _docListState = value;
                OnPropertyChanged();
            }
        }
        public CleaningModuleItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }        /// <summary>
                 /// 课件列表选中项
                 /// </summary>
        public List<CleaningModuleItem> SelectedItems
        {
            get
            {
                if (_selectedItems == null)
                {
                    _selectedItems = new List<CleaningModuleItem>();
                }

                return _selectedItems;
            }
            set
            {
                _selectedItems = value;
                OnPropertyChanged();
            }
        }


        private List<CleaningModuleItem> _selectedItems = new List<CleaningModuleItem>();
        private CleaningModuleItem _selectedItem = null;
        private EditStatus _docListState = EditStatus.Normal;
        private List<CleaningModuleItem> _itemsSource = new List<CleaningModuleItem>();

        #endregion

    }

    internal class CleanCacheBackgroundWorkerModel
    {
        public CleanCacheBackgroundWorkerModel()
        {
        }

        public List<string> ToCleaningFiles { get; set; }
    }

    internal class ProgressChangedContent
    {
        public string OperationDetail { get; set; }
        public string OperationOutput { get; set; }
        public string OperationError { get; set; }
        public string ProgressDetail { get; set; }
    }

}
