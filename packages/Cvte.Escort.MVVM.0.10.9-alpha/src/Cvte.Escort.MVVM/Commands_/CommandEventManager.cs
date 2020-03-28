using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Cvte.Escort
{
    /// <summary>
    /// 管理 <see cref="ICommand.CanExecuteChanged"/> 事件的执行时机。
    /// </summary>
    internal class CommandEventManager
    {
        /// <summary>
        /// 当执行命令的 add 方法时，调用此方法以添加 <see cref="ICommand.CanExecuteChanged"/> 事件执行时机的管理。
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="handler">命令处理函数。</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "command")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "handler")]
        internal static void AddHandler(ICommand command, Delegate handler)
        {

        }

        /// <summary>
        /// 当执行命令的 remove 方法时，调用此方法以移除 <see cref="ICommand.CanExecuteChanged"/> 事件执行时机的管理。
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="handler">命令处理函数</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "command")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "handler")]
        internal static void RemoveHandler(ICommand command, Delegate handler)
        {

        }
    }
}
