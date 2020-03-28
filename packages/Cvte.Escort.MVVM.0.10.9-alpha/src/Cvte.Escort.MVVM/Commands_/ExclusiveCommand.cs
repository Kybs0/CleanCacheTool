using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为 <see cref="ViewModelBase"/> 统一管理 <see cref="IAsyncCommand"/> 的互斥行为提供扩展方法。
    /// </summary>
    public static partial class ExclusiveCommand
    {
        /// <summary>
        /// 记录所有被管理的互斥异步命令。
        /// </summary>
        private static readonly Dictionary<ViewModelBase, HashSet<AsyncCommandBase>> ManagedAsyncCommandDictionary
            = new Dictionary<ViewModelBase, HashSet<AsyncCommandBase>>();

        /// <summary>
        /// 将某个 <see cref="ViewModelBase"/> 中创建的 <see cref="AsyncCommandBase"/> 加入到异步互斥命令集合中进行统一管理。
        /// </summary>
        /// <param name="viewModel">异步互斥命令的互斥边界，在此 ViewModel 中。</param>
        /// <param name="asyncCommand">互斥的异步命令。</param>
        private static void ManageExclusiveCommand(
            [NotNull] ViewModelBase viewModel, [NotNull] AsyncCommandBase asyncCommand)
        {
            if (!ManagedAsyncCommandDictionary.TryGetValue(viewModel, out var commands))
            {
                commands = new HashSet<AsyncCommandBase>();
                ManagedAsyncCommandDictionary[viewModel] = commands;
            }

            commands.Add(asyncCommand);
        }

        /// <summary>
        /// 找到被某个特定 <see cref="ViewModelBase"/> 互斥边界中的所有互斥的 <see cref="AsyncCommandBase"/>。
        /// </summary>
        /// <param name="viewModel">异步命令的互斥边界。</param>
        /// <returns>在指定互斥边界下的所有互斥的异步命令。</returns>
        [NotNull]
        internal static IEnumerable<AsyncCommandBase> FindExclusiveCommandsFrom([NotNull] ViewModelBase viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            return ManagedAsyncCommandDictionary.TryGetValue(viewModel, out var commands)
                ? FindPrivate()
                : Enumerable.Empty<AsyncCommandBase>();

            IEnumerable<AsyncCommandBase> FindPrivate()
            {
                foreach (var command in commands)
                {
                    yield return command;
                }
            }
        }

        /// <summary>
        /// 找到与指定 <see cref="AsyncCommandBase"/> 异步命令互斥的所有异步命令。
        /// </summary>
        /// <param name="asyncCommand">查找与之互斥的异步命令。</param>
        /// <returns>与指定异步命令互斥的所有异步命令。</returns>
        internal static IEnumerable<AsyncCommandBase> FindExclusiveCommandsFrom([NotNull] AsyncCommandBase asyncCommand)
        {
            if (asyncCommand == null) throw new ArgumentNullException(nameof(asyncCommand));

            foreach (var pair in ManagedAsyncCommandDictionary)
            {
                if (pair.Value.Contains(asyncCommand))
                {
                    return FindExclusiveCommandsFrom(pair.Key);
                }
            }

            return Enumerable.Empty<AsyncCommandBase>();
        }

        /// <summary>
        /// 创建用于执行指定异步任务 <paramref name="asyncAction"/> 的 <see cref="AsyncCommand"/>。
        /// 从同一个 <see cref="ViewModelBase"/> 创建出来的 <see cref="IAsyncCommand"/> 将在命令执行期间互斥。
        /// </summary>
        /// <param name="viewModel">异步命令将在此 <see cref="ViewModelBase"/> 范围内互斥。</param>
        /// <param name="asyncAction">异步命令中执行的异步任务。</param>
        /// <returns>具有互斥属性的 <see cref="AsyncCommand"/>。</returns>
        public static AsyncCommand CreateExclusiveCommand(
            [NotNull] this ViewModelBase viewModel, [NotNull] Func<Task> asyncAction)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

            var command = new AsyncCommand(asyncAction);
            ManageExclusiveCommand(viewModel, command);
            return command;
        }

        /// <summary>
        /// 创建用于执行指定异步任务 <paramref name="asyncAction"/> 的 <see cref="AsyncCommand"/>。
        /// 从同一个 <see cref="ViewModelBase"/> 创建出来的 <see cref="IAsyncCommand"/> 将在命令执行期间互斥。
        /// </summary>
        /// <param name="viewModel">异步命令将在此 <see cref="ViewModelBase"/> 范围内互斥。</param>
        /// <param name="asyncAction">异步命令中执行的异步任务。</param>
        /// <returns>具有互斥属性的 <see cref="AsyncCommand"/>。</returns>
        public static AsyncCommand CreateExclusiveCommand(
            [NotNull] this ViewModelBase viewModel, [NotNull] Func<AsyncExecutingContext, Task> asyncAction)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

            var command = new AsyncCommand(asyncAction);
            ManageExclusiveCommand(viewModel, command);
            return command;
        }
    }
}
