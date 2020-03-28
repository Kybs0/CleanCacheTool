using System;
using System.Threading.Tasks;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    public static partial class ExclusiveCommand
    {
        /// <summary>
        /// 创建用于执行指定异步任务 <paramref name="asyncAction"/> 的 <see cref="AsyncCommand{T}"/>。
        /// 从同一个 <see cref="ViewModelBase"/> 创建出来的 <see cref="IAsyncCommand"/> 将在命令执行期间互斥。
        /// </summary>
        /// <param name="viewModel">异步命令将在此 <see cref="ViewModelBase"/> 范围内互斥。</param>
        /// <param name="asyncAction">异步命令中执行的异步任务。</param>
        /// <returns>具有互斥属性的 <see cref="AsyncCommand{T}"/>。</returns>
        public static AsyncCommand<T> CreateExclusiveCommand<T>(
            [NotNull] this ViewModelBase viewModel, [NotNull] Func<T, Task> asyncAction)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

            var command = new AsyncCommand<T>(asyncAction);
            ManageExclusiveCommand(viewModel, command);
            return command;
        }

        /// <summary>
        /// 创建用于执行指定异步任务 <paramref name="asyncAction"/> 的 <see cref="AsyncCommand{T}"/>。
        /// 从同一个 <see cref="ViewModelBase"/> 创建出来的 <see cref="IAsyncCommand"/> 将在命令执行期间互斥。
        /// </summary>
        /// <param name="viewModel">异步命令将在此 <see cref="ViewModelBase"/> 范围内互斥。</param>
        /// <param name="asyncAction">异步命令中执行的异步任务。</param>
        /// <returns>具有互斥属性的 <see cref="AsyncCommand{T}"/>。</returns>
        public static AsyncCommand<T> CreateExclusiveCommand<T>(
            [NotNull] this ViewModelBase viewModel, [NotNull] Func<AsyncExecutingContext, T, Task> asyncAction)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

            var command = new AsyncCommand<T>(asyncAction);
            ManageExclusiveCommand(viewModel, command);
            return command;
        }
    }
}
