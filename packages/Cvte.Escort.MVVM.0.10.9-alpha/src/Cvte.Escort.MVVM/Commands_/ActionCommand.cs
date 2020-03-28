using System;
using System.Windows.Input;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为普通的动作提供 <see cref="ICommand"/> 的实现。
    /// </summary>
    public class ActionCommand : ICommand
    {
        /// <summary>
        /// 创建 <see cref="ActionCommand"/> 的新实例，当 <see cref="ICommand"/> 被执行时，将调用参数传入的动作。
        /// </summary>
        public ActionCommand([NotNull] Action action, [CanBeNull] Func<bool> canExecute = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 此 <see cref="ActionCommand"/> 中用于执行的任务本身。
        /// </summary>
        [NotNull]
        private readonly Action _action;

        /// <summary>
        /// 此 <see cref="ActionCommand"/> 中用于判定任务是否可以执行。
        /// </summary>
        [CanBeNull]
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 执行任务。
        /// </summary>
        void ICommand.Execute(object parameter)
        {
            if (parameter != null)
                throw new ArgumentException(
                    $"不能向 ActionCommand 指定参数，因为这里指定的参数无法传递。如果希望传递参数，请使用 {nameof(ActionCommand)} 的泛型版本。",
                    nameof(parameter));

            Execute();
        }

        /// <summary>
        /// 执行任务。
        /// </summary>
        public void Execute()
        {
            _action.Invoke();
        }

        /// <summary>
        /// 判断命令何时可用。
        /// </summary>
        bool ICommand.CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

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
