using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace CleanCacheTool.Api
{
    public class FileUtil
    {
        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                inUse = false;
            }
            catch (UnauthorizedAccessException)
            {
                //如果是只读，则尝试设置为默认权限，再判断文件是否可读写
                try
                {
                    File.SetAttributes(fileName, FileAttributes.Normal);
                    fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    inUse = false;
                }
                catch (Exception)
                {
                    // ignored
                }
                finally
                {
                    fs?.Close();
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
            finally
            {
                fs?.Close();
            }

            return inUse;//true表示正在使用,false没有使用  
        }

        /// <summary>
        /// 检查当前用户是否拥有此文件的操作权限
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool HasOperationPermission(string filePath)
        {
            var currentUserIdentity = Path.Combine(Environment.UserDomainName, Environment.UserName);

            FileSecurity fileAcl = File.GetAccessControl(filePath);
            var userAccessRules = fileAcl.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)).OfType<FileSystemAccessRule>().Where(i => i.IdentityReference.Value == currentUserIdentity).ToList();

            if (userAccessRules.Count > 0 &&
                userAccessRules.Any(i => (i.FileSystemRights & FileSystemRights.Delete) != 0 && i.AccessControlType == AccessControlType.Allow))
            {

                return true;
            }
            return false;
        }

        public static void Delete(string path)
        {
            //为不影响其它模块使用此文件时，转移文件操作导致占用问题，提前判断是否被占用
            if (IsFileInUse(path))
            {
                throw new InvalidOperationException($"文件{path}被占用，无法删除！");
            }
            var fileInfo = new FileInfo(path);

            //1.如果是只读，则取消只读
            if ((fileInfo.Attributes & FileAttributes.ReadOnly) > 0)
                fileInfo.Attributes ^= FileAttributes.ReadOnly;

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        /// <summary>
        /// 文件移动
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        public static void Move(string oldPath, string newPath)
        {
            //为不影响其它模块使用此文件时，转移文件操作导致占用问题，提前判断是否被占用
            if (IsFileInUse(oldPath))
            {
                throw new InvalidOperationException($"文件{oldPath}被占用，无法移动！");
            }

            if (File.Exists(newPath))
            {
                if (IsFileInUse(newPath))
                {
                    throw new InvalidOperationException($"文件{newPath}被占用，无法被覆盖！");
                }
                File.Delete(newPath);
            }
            else
            {
                var folderPath = Path.GetDirectoryName(newPath);
                FolderUtil.EnsureDirectoryExisting(folderPath);
            }

            File.Move(oldPath, newPath);
        }

        public static long GetFileSize(string file)
        {
            try
            {
                var length = new FileInfo(file).Length;
                return length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return 0;
        }
    }
}
