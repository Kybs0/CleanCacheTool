using System.Threading.Tasks;
using System.Windows;

namespace Cvte.Escort
{
    /// <inheritdoc />
    /// <summary>
    /// 为 <see cref="Cvte.Escort" /> 中所有种类的 <see cref="System.Windows.Input.ICommand" /> 提供异步执行的通用命令代理。
    /// </summary>
    public class AsyncViewCommand : ViewCommand
    {
        /// <summary>
        /// 当命令准备开始执行时发生。
        /// 如果命令的执行需要用户使用 UI 交互产生额外的参数，则在此处使用获取参数。
        /// 例如，可以在此事件中显示一个打开文件对话框，然后将所选文件的完全限定路径作为命令参数。
        /// </summary>
        public new event AsyncEventHandler<CommandStartingEventArgs> Starting;

        /// <summary>
        /// 引发异步的 <see cref="Starting"/> 事件，并在事件的执行异步结束之后继续执行命令的其他部分。
        /// </summary>
        protected override async Task<CommandStartingEventArgs> OnStarting()
        {
            var startingArgs = new CommandStartingEventArgs();
            if (Starting != null)
            {
                await Starting.Invoke(this, startingArgs).ConfigureAwait(false);
            }

            return startingArgs;
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new AsyncViewCommand
            {
                Command = Command,
            };
        }
    }
}
