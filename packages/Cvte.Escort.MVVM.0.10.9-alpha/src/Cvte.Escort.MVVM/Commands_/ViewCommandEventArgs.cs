using System;
using System.Windows.Input;
using Cvte.Escort.Annotations;
using Cvte.Escort.Commands;

namespace Cvte.Escort
{
    /// <summary>
    /// 为 ViewCommand.Starting 事件提供参数。
    /// 如果指定了 <see cref="Parameter"/> 属性，则此属性所提供的参数将传入 ViewModel 的命令参数中。
    /// </summary>
    public class CommandStartingEventArgs : EventArgs
    {
        /// <summary>
        /// 指定命令应该使用的参数。
        /// <para>可以使用任意的 object 类型的参数，这类似于为 <see cref="ICommand"/> 的执行传入普通参数。</para>
        /// <para>可以使用 <see cref="ValueTuple{T}"/> 指定元组参数；</para>
        /// <para>可以使用 <see cref="CommandParameter{T}"/> 指定只读参数；</para>
        /// <para>可以使用 <see cref="AsyncCommandParameter{T}"/> 使用异步任务获取并提供参数；</para>
        /// <para>可以使用自定义的 <see cref="IParameterProvider{T}"/> 或 <see cref="IAsyncParameterProvider{T}"/> 提供参数；</para>
        /// </summary>
        [CanBeNull]
        public object Parameter { get; set; }

        /// <summary>
        /// 获取一个值，该值指示命令应该立即取消执行。
        /// </summary>
        internal bool IsCanceled { get; private set; }

        /// <summary>
        /// 取消执行此命令。通常意味着命令需要额外的命令参数，但用户没有为此提供合适的参数。
        /// 例如，用户点击了打开文件对话框中的“取消”按钮。
        /// </summary>
        public void Cancel()
        {
            IsCanceled = true;
        }
    }

    /// <summary>
    /// 为 ViewCommand.Completed 事件提供参数。
    /// </summary>
    public class CommandCompletedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// 为 ViewCommand.ExceptionOccurred 事件提供参数。
    /// </summary>
    public class ExceptionOccurredEventArgs : EventArgs
    {
        /// <summary>
        /// 创建 <see cref="ExceptionOccurredEventArgs"/> 的新实例。
        /// </summary>
        public ExceptionOccurredEventArgs([NotNull] Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        /// <summary>
        /// 获取命令执行过程中发生的异常；可以为 null，说明此命令执行的中断不是由异常导致的，而是主动中断。
        /// </summary>
        [NotNull]
        public Exception Exception { get; }

        /// <summary>
        /// 获取命令执行过程中发生异常时，代表具体异常的标识符；可以为 null，说明此时发生了常规的异常，没有指定标识符。
        /// </summary>
        [CanBeNull]
        public string Identifier { get; set; }
    }
}
