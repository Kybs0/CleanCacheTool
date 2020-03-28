using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;
using Cvte.Escort.Annotations;
using Cvte.Escort.Commands;

namespace Cvte.Escort
{
    /// <summary>
    /// 表示一个必须提供参数才能执行的命令。
    /// </summary>
    public class ActionCommand<T> : ICommand
    {
        /// <summary>
        /// 创建 <see cref="ActionCommand"/> 的新实例，当 <see cref="ICommand"/> 被执行时，将调用参数传入的动作。
        /// </summary>
        public ActionCommand([NotNull] Action<T> action, Func<bool> canExecute = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 用于接受所提供的参数并执行的委托。
        /// 只可能是 Action{T} 或 Func{T, Task}。
        /// </summary>
        [NotNull]
        private readonly Action<T> _action;

        /// <summary>
        /// 此 <see cref="ActionCommand{T}"/> 中用于判定任务是否可以执行。
        /// </summary>
        [CanBeNull]
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 使用指定的参数执行此命令。
        /// 框架中没有约定参数值是否允许为 null，这由参数定义时的泛型类型约定（C#8.0）或由命令的实现者约定。
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t")]
        public void Execute(T t)
        {
            _action(t);
        }

#if GENERATED_CODE
        /// <summary>
        /// 尝试以异步的方式执行此同步命令。因为参数的获取可能是异步的（例如使用 <see cref="IAsyncParameterProvider{T}"/>），所以此方法也必须是异步的。
        /// 对于传入的参数会被解析成多个参数，传入的参数一定不允许为 null。
        /// </summary>
        /// <param name="parameter">接口中传入的原始参数。</param>
#else
        /// <summary>
        /// 尝试以异步的方式执行此同步命令。因为参数的获取可能是异步的（例如使用 <see cref="IAsyncParameterProvider{T}"/>），所以此方法也必须是异步的。
        /// 对于单个泛型参数的 <see cref="ActionCommand{T}"/> 而言，传入的参数由业务定义含义，所以不能保证 null 值的合理性。
        /// </summary>
        /// <param name="parameter">接口中传入的原始参数。</param>
#endif
        [NotNull]
        public async Task ExecuteAsync(
#if GENERATED_CODE
            [NotNull]
#endif
            object parameter)
        {
            var (t, canExecute) = await ParameterExtractor.ExtractAsync<T>(parameter).ConfigureAwait(true);
            if (canExecute)
            {
                Execute(t);
            }
        }

        void ICommand.Execute(
#if GENERATED_CODE
            [NotNull]
#endif
            object parameter)
        {
            ExecuteAsync(parameter).ConfigureAwait(false);
        }

        bool ICommand.CanExecute(
#if GENERATED_CODE
            [NotNull]
#endif
            object parameter) => _canExecute?.Invoke() ?? true;

        /// <inheritdoc />
        /// <summary>
        /// 当命令的可执行性改变时发生。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandEventManager.AddHandler(this, value);
            }
            remove
            {
                if (_canExecute != null)
                    CommandEventManager.RemoveHandler(this, value);
            }
        }
    }
}
