using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CleanCacheTool.Api
{
    public static class FolderUtil
    {
        /// <summary>
        /// 递归删除文件夹，包括其中的所有文件和文件夹。
        /// </summary>
        /// <remarks>
        /// 这是不使用系统自身的递归删除方法`Directory.Delete(dirPath, true)`是因为此方法有坑。
        /// 可能导致文件删除失败之后再无访问权限。
        /// </remarks>
        /// <param name="dirPath"></param>
        public static void DeleteFolder(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                return;
            }

            var directory = dirPath;
            var subdirectories = new Stack<string>();
            subdirectories.Push(directory);

            var exceptionList = new List<Exception>();

            var folderList = new List<string>();

            // 尽可能地删除目录中的文件。

            // 如果出现异常也不需要记录
            while (subdirectories.Any())
            {
                var dir = subdirectories.Pop();
                folderList.Add(dir);

                try
                {
                    var files = Directory.GetFiles(dir);
                    foreach (var file in files)
                    {
                        try
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                        }
                        catch (Exception e)
                        {
                            exceptionList.Add(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);
                }

                try
                {
                    var subdirs = Directory.GetDirectories(dir);
                    foreach (var subdir in subdirs)
                    {
                        subdirectories.Push(subdir);
                    }
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);
                }
            }

            // 删除目录结构。
            try
            {
                for (var i = folderList.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        Directory.Delete(folderList[i], true);
                    }
                    catch (Exception e)
                    {
                        exceptionList.Add(e);
                    }
                }

                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch (Exception e)
            {
                exceptionList.Add(e);
            }
        }
        /// <summary>
        /// 获取选择文件夹的对话框
        /// </summary>
        /// <returns></returns>
        public static FolderBrowserDialog GetFolderBrowserDialog()
        {
            var dialog = new FolderBrowserDialog();
            return dialog;
        }

        /// <summary>
        /// 移动文件夹
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        public static void MoveFolder(string sourceFolder, string destFolder)
        {
            sourceFolder = EnsureDirectoryExisting(sourceFolder);
            destFolder = EnsureDirectoryExisting(destFolder);
            Directory.Move(sourceFolder, destFolder);
        }

        /// <summary>
        /// 确保一定存在一个目录。如果不存在，会自动创建。
        /// </summary>
        /// <param name="directory">要确保存在的目录。</param>
        /// <returns>已保证存在的目录，即参数中传入的目录。</returns>
        public static string EnsureDirectoryExisting(string directory)
        {
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (IOException ex)
                {
                }
            }
            return directory;
        }

        /// <summary>
        /// 获取目录中的占用空间大小
        /// </summary>
        /// <param name="folder"></param>
        public static long GetFolderSpaceSize(string folder)
        {
            long totalSpaceSize = 0;
            var directoryInfo = new DirectoryInfo(folder);

            //文件占用空间
            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                long fileInfoSize = 0;
                try
                {
                    fileInfoSize = GetFolderSpaceSize(fileInfo.FullName);
                }
                catch (Exception e)
                {

                }
                totalSpaceSize += fileInfoSize;
            }

            //子目录占用空间
            foreach (var subdirectoryInfo in directoryInfo.GetDirectories())
            {
                long folderSpaceSize = 0;
                try
                {
                    folderSpaceSize = GetFolderSpaceSize(subdirectoryInfo.FullName);
                }
                catch (Exception e)
                {
                    
                }
                totalSpaceSize += folderSpaceSize;
            }

            return totalSpaceSize;
        }

        /// <summary>
        /// 检查当前用户是否拥有此文件夹的删除操作权限
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static bool HasDeleteOperationPermission(string folder)
        {
            var currentUserIdentity = Path.Combine(Environment.UserDomainName, Environment.UserName);

            DirectorySecurity fileAcl = Directory.GetAccessControl(folder);
            var userAccessRules = fileAcl.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>().Where(i => i.IdentityReference.Value == currentUserIdentity).ToList();

            if (userAccessRules.Count > 0 &&
                userAccessRules.Any(i => (i.FileSystemRights & FileSystemRights.Delete) != 0 && i.AccessControlType == AccessControlType.Allow))
            {

                return true;
            }
            return false;
        }

        public static List<string> GetAllFiles(string folder)
        {
            var list = new List<string>();
            try
            {
                if (Directory.Exists(folder))
                {
                    var directoryInfo = new DirectoryInfo(folder);
                    //文件
                    list.AddRange(directoryInfo.GetFiles().Select(i => i.FullName));
                    //文件夹
                    directoryInfo.GetDirectories().ToList().ForEach(i => list.AddRange(GetAllFiles(i.FullName)));
                }
            }
            catch (Exception e)
            {
                
            }

            return list;
        }
        public static List<string> GetAllFiles(string folder, out long totalSize)
        {
            var list = new List<string>();
            totalSize = 0;
            try
            {
                var directoryInfo = new DirectoryInfo(folder);
                //文件
                var fileInfos = directoryInfo.GetFiles();
                totalSize += fileInfos.Sum(i => i.Length);
                list.AddRange(fileInfos.Select(i => i.FullName));
                //文件夹
                foreach (var subFolder in directoryInfo.GetDirectories())
                {
                    var files = GetAllFiles(subFolder.FullName, out long subTotalSize);
                    totalSize += subTotalSize;
                    list.AddRange(files);
                }
            }
            catch (Exception e)
            {

            }

            return list;
        }

        public static List<string> GetSubFoldersAndFiles(string folder)
        {
            var list = new List<string>();

            var directoryInfo = new DirectoryInfo(folder);
            //文件
            var fileInfos = directoryInfo.GetFiles();
            list.AddRange(fileInfos.Select(i => i.FullName));
            //文件夹
            var directoryInfos = directoryInfo.GetDirectories();
            list.AddRange(directoryInfos.Select(i => i.FullName));

            return list;
        }

        /// <summary>
        /// 删除文件夹下的所有空文件夹
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="deleteSelf">是否删除自身文件夹，默认true</param>
        public static void DeleteEmptyFolder(string folder, bool deleteSelf = true)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            var folderInfo = new DirectoryInfo(folder);
            //先删除子文件夹
            var subDirectories = folderInfo.GetDirectories();
            foreach (var directoryInfo in subDirectories)
            {
                DeleteEmptyFolder(directoryInfo.FullName);
            }
            //再删除父文件夹
            var subFiles = folderInfo.GetFiles();
            var newSubDirectories = folderInfo.GetDirectories();
            if (subFiles.Length == 0 && newSubDirectories.Length == 0 && deleteSelf)
            {
                Directory.Delete(folder);
            }
        }

        /// <summary>
        /// 检查当前用户是否拥有此文件夹的操作权限
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static bool HasOperationPermission(string folder)
        {
            var currentUserIdentity = Path.Combine(Environment.UserDomainName, Environment.UserName);

            DirectorySecurity fileAcl = Directory.GetAccessControl(folder);
            var userAccessRules = fileAcl.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>().Where(i => i.IdentityReference.Value == currentUserIdentity).ToList();

            return userAccessRules.All(i => i.AccessControlType != AccessControlType.Deny);
        }
    }
}
