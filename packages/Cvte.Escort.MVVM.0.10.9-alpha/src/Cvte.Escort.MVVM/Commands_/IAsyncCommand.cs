using System.Threading.Tasks;
using System.Windows.Input;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 表示命令的执行是异步进行的。
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// 指示当前命令是否正在执行异步任务。
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 异步执行此命令并等待其返回。
        /// </summary>
        /// <param name="parameter">异步命令执行过程中需要传递给命令执行方的数据。</param>
        [NotNull]
        Task ExecuteAsync(object parameter);
    }
}
