using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CleanCacheTool.Api;
using CleanCacheTool.Controls;
using CleanCacheTool.Helper;
using CleanCacheTool.Properties;
using Microsoft.Win32;

namespace CleanCacheTool.Views.Models
{
    public class DiskSlimmingViewModel : NotifyPropertyChanged
    {
        private static DiskSlimmingViewModel _cleanCacheVeiwModel = null;

        public static DiskSlimmingViewModel Instance =>
            _cleanCacheVeiwModel ?? (_cleanCacheVeiwModel = new DiskSlimmingViewModel());
        public DiskSlimmingViewModel()
        {
            SelectDesktopCommand = new ActionCommand(SelectDesktop);
            TransferDesktopCommand = new ActionCommand(TransferDesktop);
            SetDefaultDesktopCommand = new ActionCommand(SetDefaultDesktop);
            DesktopFolder = GetCurrentDesktopFolder();
        }

        private void SetDefaultDesktop()
        {
            DestFolder = GetDefaultDektopFolder();
        }

        /// <summary>
        /// 选择桌面
        /// </summary>
        private void SelectDesktop()
        {
            using (var dialog = FolderUtil.GetFolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var destFolder = dialog.SelectedPath;
                    var isEndsWith = destFolder.EndsWith("Desktop");
                    DestFolder = isEndsWith ? destFolder : Path.Combine(destFolder, "Desktop");
                }
            }
        }

        public ICommand SetDefaultDesktopCommand { get; }
        public ICommand SelectDesktopCommand { get; }

        public ICommand TransferDesktopCommand { get; }
        /// <summary>
        /// 转移桌面
        /// </summary>
        private async void TransferDesktop()
        {
            try
            {
                var destFolder = DestFolder;
                if (string.IsNullOrWhiteSpace(destFolder))
                {
                    MessageBox.Show("目录路径不能为空");
                    return;
                }
                var currentDesktopFolder = DesktopFolder;
                if (destFolder == currentDesktopFolder)
                {
                    MessageBox.Show("目录路径与当前桌面路径相同");
                    return;
                }

                //destFolder = Path.Combine(destFolder, "Desktop");
                FolderUtil.EnsureDirectoryExisting(destFolder);
                var canChangeDataPath = await CheckCanChangeDataPath(currentDesktopFolder, destFolder);
                if (canChangeDataPath)
                {
                    MoveCacheWidthBackgroundWorker(currentDesktopFolder, destFolder);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message);
                System.Windows.MessageBox.Show(e.Message);
            }
        }

        #region 移动文件

        private void MoveCacheWidthBackgroundWorker(string currentDesktopFolder, string destDataPathFolder)
        {
            var workerModel = new MoveCacheBackgroundWorkerModel()
            {
                DeskTopFolder = currentDesktopFolder,
                DesktopFoldersAndFiles = FolderUtil.GetSubFoldersAndFiles(currentDesktopFolder),
                UserDefinedFolder = destDataPathFolder,
            };

            using (var backgroundWorker = new BackgroundWorker() { WorkerReportsProgress = true })
            {
                backgroundWorker.ProgressChanged += MoveCacheBackgroundWorker_ProgressChanged;
                backgroundWorker.DoWork += MoveCacheBackgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += MoveCacheBackgroundWorker_RunWorkerCompleted;
                backgroundWorker.RunWorkerAsync(workerModel);
            }
        }

        protected void MoveCacheBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var workerModel = (MoveCacheBackgroundWorkerModel)e.Argument;

            IsMovingFolder = true;
            worker.ReportProgress(0);

            try
            {
                long handledSize = 0;
                //1.获取所有待转移的缓存列表
                var currentOperationDetail = "获取所有待转移的文件..";
                worker.ReportProgress(0, new ProgressChangedContent()
                {
                    OperationDetail = currentOperationDetail
                });
                var allCacheFiles = FolderUtil.GetAllFiles(workerModel.DeskTopFolder, out long usertotalSize);
                MoveCacheFiles(allCacheFiles, ref handledSize, workerModel, usertotalSize, worker);
            }
            catch (Exception exception)
            {
                RestoreCacheFilesToPreviousPath(workerModel);
                LogHelper.Error(exception.Message);
                throw;
            }

            //返回执行结果
            e.Result = workerModel;
        }

        private void MoveCacheFiles(List<string> allCacheFiles, ref long handledSize, MoveCacheBackgroundWorkerModel workerModel,
            long totalSize, BackgroundWorker worker)
        {
            var currentOperationDetail = $"正在转移。。。";
            worker.ReportProgress(-1, new ProgressChangedContent()
            {
                OperationDetail = currentOperationDetail,
            });

            var currentOperationError = string.Empty;
            int filesCount = allCacheFiles.Count;
            int moveIndex = 0;
            foreach (var cacheFilePath in allCacheFiles)
            {
                try
                {
                    moveIndex++;
                    handledSize += new FileInfo(cacheFilePath).Length;

                    //移动文件
                    var newFilePath = GetNewFilePath(cacheFilePath, workerModel.DeskTopFolder, workerModel.UserDefinedFolder);
                    FileUtil.Move(cacheFilePath, newFilePath);

                    //记录已经转移的缓存文件
                    workerModel.MovedFiles.Add(new MovedFileDataMode(cacheFilePath, newFilePath));

                    //删除原有文件
                    FileUtil.Delete(cacheFilePath);


                }
                catch (Exception e)
                {
                    LogHelper.Error(e.Message);
                    currentOperationError = e.Message;
                    throw;
                }
                finally
                {
                    var cleanCacheProgressDetail = $"{moveIndex}/{filesCount}";
                    //报告进度
                    var progress = totalSize == 0 ? 100 : handledSize * 100 / totalSize;
                    worker.ReportProgress(Convert.ToInt32(progress), new ProgressChangedContent()
                    {
                        ProgressDetail = cleanCacheProgressDetail,
                        OperationError = currentOperationError
                    });
                }
            }

            //删除产品文件夹及其下所有的空文件夹
            FolderUtil.DeleteEmptyFolder(workerModel.DeskTopFolder, false);
            //报告进度
            worker.ReportProgress(-1, new ProgressChangedContent()
            {
                OperationOutput = $"转移完成.."
            });
        }

        private char[] _folderEndTrimChars = new char[1] { '\\' };
        private string GetNewFilePath(string cacheFilePath, string previousDataPath, string userDefinedPath)
        {
            var priviousDataFolder = previousDataPath.TrimEnd(_folderEndTrimChars);
            var userDefinedFolder = userDefinedPath.TrimEnd(_folderEndTrimChars);

            var partFilePath = cacheFilePath.Replace(priviousDataFolder, string.Empty);
            var newFilePath = userDefinedFolder + partFilePath;

            return newFilePath;
        }

        /// <summary>
        /// 恢复已经转移的缓存文件
        /// </summary>
        /// <param name="workerModel"></param>
        private void RestoreCacheFilesToPreviousPath(MoveCacheBackgroundWorkerModel workerModel)
        {
            try
            {
                foreach (var data in workerModel.MovedFiles)
                {
                    File.Move(data.NewFilePath, data.PreviousFilePath);
                }
                //删除文件夹下所有的空文件夹
                FolderUtil.DeleteEmptyFolder(workerModel.UserDefinedFolder, false);
            }
            catch (Exception e)
            {
                LogHelper.Error("恢复缓存失败，异常信息为：" + e.Message);
            }
        }

        /// <summary>
        /// 更新显示进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MoveCacheBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0)
            {
                MovingProgressValue = e.ProgressPercentage == 0 ? 1 : e.ProgressPercentage;
            }
            if (e.UserState is ProgressChangedContent progressChangedContent)
            {
                if (!string.IsNullOrEmpty(progressChangedContent.ProgressDetail))
                {
                    MovingProgressDetail = progressChangedContent.ProgressDetail;
                }
                if (!string.IsNullOrEmpty(progressChangedContent.OperationDetail))
                {
                    CurrentOperationDetail = progressChangedContent.OperationDetail;
                }
            }
        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MoveCacheBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DestFolder = string.Empty;
            if (e.Error != null)
            {
                MovingProgressValue = 0;
                IsMovingFolder = false;
                //用户提示
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                //转移缓存文件成功
                var movingModel = (MoveCacheBackgroundWorkerModel)e.Result;
                DesktopFolder = movingModel.UserDefinedFolder;
                MovingProgressValue = 100;
                IsMovingFolder = false;

                ModifyDesktopRegistryLocation(movingModel.UserDefinedFolder);
                if (MessageBox.Show(Resources.DiskSlimming_MoveCache_RunWorkerCompleted, "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    RestartPc();
                }
            }
        }

        private void RestartPc()
        {
            System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
            myProcess.StartInfo.FileName = "cmd.exe";//启动cmd命令
            myProcess.StartInfo.UseShellExecute = false;//是否使用系统外壳程序启动进程
            myProcess.StartInfo.RedirectStandardInput = true;//是否从流中读取
            myProcess.StartInfo.RedirectStandardOutput = true;//是否写入流
            myProcess.StartInfo.RedirectStandardError = true;//是否将错误信息写入流
            myProcess.StartInfo.CreateNoWindow = true;//是否在新窗口中启动进程
            myProcess.Start();//启动进程
            myProcess.StandardInput.WriteLine("shutdown -r -t 0");//执行重启计算机命令
        }

        private bool CreateMklinks(string currentFolder, string destFolder, List<string> movingModelDesktopFoldersAndFiles)
        {
            var defaultDektopFolder = GetDefaultDektopFolder();
            //只有桌面转移到其它路径，才添加软链接
            if (currentFolder == defaultDektopFolder)
            {
                var priviousDataFolder = currentFolder.TrimEnd(_folderEndTrimChars);
                var userDefinedFolder = destFolder.TrimEnd(_folderEndTrimChars);
                foreach (var itemPath in movingModelDesktopFoldersAndFiles)
                {
                    var extension = Path.GetExtension(itemPath);
                    var partFilePath = itemPath.Replace(priviousDataFolder, string.Empty);
                    var newFilePath = userDefinedFolder + partFilePath;
                    if (string.IsNullOrWhiteSpace(extension))
                    {
                        var commands = $"mklink /D {itemPath} {newFilePath}";
                        ExecuteCmdHelper.ExecuteCmd(commands);
                    }
                    else if (extension != ".ink")
                    {
                        var commands = $"mklink /H {itemPath} {newFilePath}";
                        ExecuteCmdHelper.ExecuteCmd(commands);
                    }
                }

                return true;
            }

            return false;
        }
        private void ModifyDesktopRegistryLocation(string path)
        {
            RegistryKey userShellFoldersKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).OpenSubKey(@"Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
            RegistryKey shellFoldersKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).OpenSubKey(@"Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", true);
            shellFoldersKey.SetValue("Desktop", path);
            userShellFoldersKey.SetValue("Desktop", path);
        }

        #endregion

        #region 检查

        /// <summary>
        /// 验证是否能更改目录
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="userDefinedPath"></param>
        /// <returns></returns>
        protected async Task<bool> CheckCanChangeDataPath(string sourceFolder, string userDefinedPath)
        {
            //当前系统用户是否拥有目标路径的操作权限
            if (!CheckPermission(userDefinedPath))
            {
                return false;
            }

            //目标磁盘可用空间 是否满足 当前用户课件缓存
            var cacheSpaceSize = FolderUtil.GetFolderSpaceSize(sourceFolder);
            if (!IsDriveSpaceSatisfyUserCache(userDefinedPath, cacheSpaceSize))
            {
                return false;
            }

            if (!Directory.Exists(userDefinedPath))
            {
                MessageBox.Show("目标路径不存在！");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 当前系统用户是否拥有目标路径的操作权限
        /// </summary>
        /// <param name="destDataPathFolder"></param>
        /// <returns></returns>
        private bool CheckPermission(string destDataPathFolder)
        {
            bool hasPermission = FolderUtil.HasOperationPermission(destDataPathFolder);
            if (!hasPermission)
            {
                //MessageBox.Show(this, "没有操作权限");
            }

            return hasPermission;
        }

        /// <summary>
        /// 目标磁盘可用空间是否满足用户缓存
        /// </summary>
        /// <param name="destDataPathFolder"></param>
        /// <param name="requiredSpace"></param>
        /// <returns></returns>
        private bool IsDriveSpaceSatisfyUserCache(string destDataPathFolder, long requiredSpace)
        {
            //所在磁盘可用空间
            var driveAvailableSize = DriveUtil.GetDriveAvailableSizeByPath(destDataPathFolder);

            var hasAvaliableSpace = driveAvailableSize > requiredSpace;
            if (!hasAvaliableSpace)
            {
                //MessageBox.Show(this, "没有可用空间");
            }
            return hasAvaliableSpace;
        }

        #endregion

        #region 辅助方法

        public string GetCurrentDesktopFolder()
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return folderPath;
        }

        public string GetDefaultDektopFolder()
        {
            var folderPath = Path.Combine(@"C:\Users\", Environment.UserName, "Desktop");
            return folderPath;
        }

        #endregion

        #region 属性

        private string _desktopFolder = string.Empty;
        public string DesktopFolder
        {
            get => _desktopFolder;
            set
            {
                _desktopFolder = value;
                OnPropertyChanged();
            }
        }
        private string _destFolder = string.Empty;
        public string DestFolder
        {
            get => _destFolder;
            set
            {
                _destFolder = value;
                OnPropertyChanged();
            }
        }
        private bool _isMovingFolder = false;
        public bool IsMovingFolder
        {
            get => _isMovingFolder;
            set
            {
                _isMovingFolder = value;
                OnPropertyChanged();
            }
        }

        private int _movingProgressValue = 0;
        public int MovingProgressValue
        {
            get => _movingProgressValue;
            set
            {
                _movingProgressValue = value;
                OnPropertyChanged();
            }
        }

        private string _movingProgressDetail;
        public string MovingProgressDetail
        {
            get => _movingProgressDetail;
            set
            {
                _movingProgressDetail = value;
                OnPropertyChanged();
            }
        }

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
    }
    internal class MoveCacheBackgroundWorkerModel
    {
        /// <summary>
        /// 待转移的数据缓存路径
        /// </summary>
        public string DeskTopFolder { get; set; }

        /// <summary>
        /// 当前桌面的文件夹与文件列表
        /// </summary>
        public List<string> DesktopFoldersAndFiles { get; set; }

        /// <summary>
        /// 用户选择的路径
        /// </summary>
        public string UserDefinedFolder { get; set; }

        /// <summary>
        /// 当前已经转移的缓存文件列表
        /// </summary>
        public List<MovedFileDataMode> MovedFiles = new List<MovedFileDataMode>();
    }
    /// <summary>
    /// 转移的缓存文件
    /// </summary>
    internal class MovedFileDataMode
    {

        public MovedFileDataMode(string previousFilePath, string newFilePath)
        {
            PreviousFilePath = previousFilePath;
            NewFilePath = newFilePath;
        }
        public string PreviousFilePath { get; set; }
        public string NewFilePath { get; set; }
    }
}
