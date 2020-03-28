using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为 <see cref="Cvte.Escort" /> 中所有种类的 <see cref="ICommand" /> 提供通用的命令代理。
    /// </summary>
    public class ViewCommand : Freezable, ICommand
    {
        /// <summary>
        /// 标识 <see cref="Command"/> 的依赖项属性。
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command", typeof(ICommand), typeof(ViewCommand),
            new PropertyMetadata(default(ICommand), (o, args) =>
                ((ViewCommand) o).OnCommandChanged((ICommand) args.OldValue, (ICommand) args.NewValue)));

        /// <summary>
        /// 请使用 <see cref="System.Windows.Data.Binding"/> 从 ViewModel 中获取一个命令，并将此命令设置到此属性中。
        /// 如果此属性没有绑定或绑定过程失败，则此属性为 null。
        /// </summary>
        [CanBeNull, DefaultValue(null)]
        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// 当 <see cref="Command"/> 属性改变时。
        /// </summary>
        private void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
        {
            // 移除旧的 Command 的 CanExecuteChanged 事件。
            if (oldCommand != null)
            {
                WeakEventManager<ICommand, EventArgs>.RemoveHandler(oldCommand,
                    nameof(ICommand.CanExecuteChanged), OnCanExecuteChanged);
            }

            // 添加新的 Command 的 CanExecuteChanged 事件。
            if (newCommand != null)
            {
                WeakEventManager<ICommand, EventArgs>.AddHandler(newCommand,
                    nameof(ICommand.CanExecuteChanged), OnCanExecuteChanged);
            }

            OnCanExecuteChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// 当命令准备开始执行时发生。
        /// 如果命令的执行需要用户使用 UI 交互产生额外的参数，则在此处使用获取参数。
        /// 例如，可以在此事件中显示一个打开文件对话框，然后将所选文件的完全限定路径作为命令参数。
        /// </summary>
        public event EventHandler<CommandStartingEventArgs> Starting;

        /// <summary>
        /// 当命令执行过程中发生异常时发生。
        /// 无论时普通的命令还是异步命令，发生的异常都会被捕捉并在此事件中报告给 UI。
        /// 通常，UI 应该根据具体的异常信息进行不同种类的 UI 提示。
        /// </summary>
        [PublicAPI]
        public event EventHandler<ExceptionOccurredEventArgs> ExceptionOccurred;

        /// <summary>
        /// 当命令执行结束时发生，但如果命令中发生了异常，则不会发生此事件。
        /// 通常可以在此处执行一些收尾的 UI 或交互。
        /// 例如，可以在此事件中显示一个提示框，告知用户文件已经成功保存到指定的位置。
        /// </summary>
        [PublicAPI]
        public event EventHandler<CommandCompletedEventArgs> Completed;

        /// <summary>
        /// 当命令的可执行性改变时发生。
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 当 <see cref="Command"/> 命令中的可执行性改变时发生。
        /// 引发此命令的可执行性。
        /// </summary>
        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }

        void ICommand.Execute(object commandParameter)
        {
            ExecuteCore(commandParameter).ConfigureAwait(false);
        }

        /// <summary>
        /// 执行命令。注意：由于内部绑定的命令有可能是异步命令，所以此方法可能会在异步方法返回前返回。
        /// 这将开始执行其内部绑定的命令，而内部命令的参数将从 <see cref="Starting"/> 事件中获取。
        /// </summary>
        [PublicAPI]
        public void Execute()
        {
            ExecuteCore(null).ConfigureAwait(false);
        }

        /// <summary>
        /// 异步等待命令的执行。这将开始执行其内部绑定的命令，而内部命令的参数将从 <see cref="Starting"/> 事件中获取。
        /// </summary>
        [PublicAPI]
        public async Task ExecuteAsync()
        {
            await ExecuteCore(null).ConfigureAwait(false);
        }

        private async Task ExecuteCore(object commandParameter)
        {
            var command = Command;
            if (command == null)
                return;
            await ExecuteAsync(command, commandParameter).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(ICommand command, object commandParameter)
        {
            // 引发 Starting 事件
            var startingArgs = await OnStarting().ConfigureAwait(true);
            if (startingArgs.IsCanceled)
            {
                return;
            }

            // 如果在 Starting 事件中指定了参数，则覆盖命令本身传入的参数。
            commandParameter = startingArgs.Parameter ?? commandParameter;

            // 对于异步命令，需要异步 catch；否则直接 catch。
            try
            {
                if (command is IAsyncCommand asyncCommand)
                {
                    await asyncCommand.ExecuteAsync(commandParameter).ConfigureAwait(true);
                }
                else
                {
                    command.Execute(commandParameter);
                }
                Completed?.Invoke(this, new CommandCompletedEventArgs());
            }
            catch (Exception ex)
            {
                ExceptionOccurred?.Invoke(this, new ExceptionOccurredEventArgs(ex));
            }
        }

        /// <summary>
        /// 在派生类中重写此方法时，引发 <see cref="Starting"/> 事件。
        /// </summary>
        protected virtual Task<CommandStartingEventArgs> OnStarting()
        {
            var startingArgs = new CommandStartingEventArgs();
            Starting?.Invoke(this, startingArgs);
            return Task.FromResult(startingArgs);
        }

        /// <inheritdoc />
        /// <summary>
        /// 使用原始命令的 <see cref="ICommand.CanExecute(Object)" /> 以确认可执行性。
        /// </summary>
        bool ICommand.CanExecute(object parameter)
        {
            return Command?.CanExecute(parameter) is true;
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new ViewCommand
            {
                Command = Command,
            };
        }
    }
}
