using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为异步任务提供 <see cref="ICommand" />。
    /// </summary>
    public abstract class AsyncCommandBase : BindableObject, IAsyncCommand, IAsyncProgress
    {
        /// <summary>
        /// 当派生类使用此构造函数构造父类时，指定异步任务的重新进入策略。
        /// 派生类需要自己处理异步任务本身，需要重写 <see cref="ExecuteAsyncCore"/> 方法来执行异步任务。
        /// </summary>
        /// <param name="reentrancyPolicy">异步任务的重新进入策略。</param>
        protected AsyncCommandBase(ReentrancyPolicy reentrancyPolicy = default)
        {
            if (!Enum.IsDefined(typeof(ReentrancyPolicy), reentrancyPolicy))
            {
                throw new InvalidEnumArgumentException(
                    nameof(reentrancyPolicy), (int) reentrancyPolicy, typeof(ReentrancyPolicy));
            }

            _reentrancyInvoker = ReentrancyStateMachine.FromPolicy(ExecuteWithStatesAsync, reentrancyPolicy);
            ReentrancyPolicy = reentrancyPolicy;
        }

        /// <summary>
        /// 用于决定异步任务执行时机的重新进入策略状态机。
        /// </summary>
        private readonly ReentrancyStateMachine _reentrancyInvoker;

        /// <summary>
        /// 获取此命令中异步任务的重新进入策略。
        /// </summary>
        public ReentrancyPolicy ReentrancyPolicy { get; }

        /// <inheritdoc />
        /// <summary>
        /// 指示当前命令是否正在执行异步任务。
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            private set => SetValue(ref _isRunning, value);
        }

        /// <summary>
        /// 指示当前命令中异步任务的执行进度，值范围为 [0,1]，如果实现者没有报告进度，则执行完毕之前会一直保持 0，执行完后为 1。
        /// </summary>
        public double Progress
        {
            get => _progress;
            private set => SetValue(ref _progress, value);
        }

        /// <summary>
        /// 获取一个值，该值指示当前此异步命令是否可以被执行。
        /// </summary>
        public bool CanExecute
        {
            get => _canExecute;
            private set
            {
                if (Equals(_canExecute, value)) return;

                _canExecute = value;
                OnCanExecuteChanged();
            }
        }

        /// <summary>
        /// 重新评估 <see cref="CanExecute"/> 属性的值。
        /// 如果此前强制设置过 <see cref="CanExecute"/> 的值，那么通过此方法可以清除强制设置的值，让其恢复自动管理的状态。
        /// </summary>
        private void ReevaluateCanExecute()
        {
            // TODO 目前只考虑了互斥命令，尚未考虑异步任务的重新进入。
            CanExecute = !AnyExclusiveCommandsRunning;
        }

        /// <inheritdoc />
        /// <summary>
        /// 执行此命令指定的异步任务，但不会等待。
        /// 如果任务已经在执行，则根据 <see cref="ReentrancyPolicy"/> 指定的重新进入策略重新进入。
        /// </summary>
        /// <param name="parameter">由 <see cref="ICommand"/> 接口传入的参数。</param>
        void ICommand.Execute(object parameter)
        {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            ExecuteAsync(parameter);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }

        /// <inheritdoc />
        /// <summary>
        /// 执行此命令指定的异步任务。
        /// 如果任务已经在执行，则根据 <see cref="ReentrancyPolicy" /> 指定的重新进入策略重新进入。
        /// </summary>
        /// <param name="parameter">由 <see cref="ICommand" /> 接口传入的参数。</param>
        async Task IAsyncCommand.ExecuteAsync(object parameter)
        {
            await ExecuteAsync(parameter).ConfigureAwait(false);
        }

        /// <summary>
        /// 在派生类中调用此方法以执行此命令指定的异步任务。
        /// 如果任务已经在执行，则根据 <see cref="ReentrancyPolicy"/> 指定的重新进入策略重新进入。
        /// </summary>
        /// <param name="parameter">由 <see cref="ICommand"/> 接口传入的参数。</param>
        [NotNull]
        protected async Task ExecuteAsync(object parameter)
        {
            if (AnyExclusiveCommandsRunning) return;

            ReportOtherExclusiveCommands(cmd => cmd.CanExecute = false);
            try
            {
                await _reentrancyInvoker.Invoke(parameter).ConfigureAwait(true);
            }
            finally
            {
                ReportOtherExclusiveCommands(cmd => cmd.ReevaluateCanExecute());
            }
        }

        /// <summary>
        /// 执行 <see cref="ExecuteAsyncCore"/> 方法，并在起止时更新进度和状态。
        /// 注意：此方法由 <see cref="ReentrancyStateMachine"/> 决定调用时机以便处理异步任务的重新进入。
        /// </summary>
        /// <param name="parameter">由 <see cref="ICommand"/> 接口传入的参数。</param>
        [NotNull]
        private async Task ExecuteWithStatesAsync(object parameter)
        {
            Progress = 0.0;
            IsRunning = true;
            try
            {
                await ExecuteAsyncCore(parameter).ConfigureAwait(true);
                Progress = 1.0;
            }
            catch (Exception)
            {
                Progress = 0.0;
                throw;
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// 派生类重写此方法时，实际执行异步任务。
        /// </summary>
        /// <param name="parameter">由 <see cref="ICommand"/> 接口传入的参数。</param>
        [NotNull]
        protected abstract Task ExecuteAsyncCore(object parameter);

        /// <summary>
        /// 尚未实现。取消命令中正在执行的异步任务。
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        /// <summary>
        /// 为异步任务报告进度百分比，取值范围为 [0, 1]。
        /// </summary>
        /// <param name="progress">进度百分比，取值范围为 [0, 1]。</param>
        void IAsyncProgress.ReportProgress(double progress)
        {
            if (progress < 0 || progress > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(progress), "Progress can only be in range of 0 to 1.");
            }

            Progress = progress;
        }

        /// <inheritdoc />
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute;
        }

        /// <inheritdoc />
        /// <summary>
        /// 当命令的可执行性改变时发生。
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 引发 <see cref="CanExecuteChanged"/> 事件。
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool AnyExclusiveCommandsRunning =>
            ExclusiveCommand.FindExclusiveCommandsFrom(this).Any(x => x.IsRunning);

        private void ReportOtherExclusiveCommands([NotNull] Action<AsyncCommandBase> report)
        {
            foreach (var command in ExclusiveCommand.FindExclusiveCommandsFrom(this).Where(x => x != this))
            {
                report(command);
            }
        }

        [ContractPublicPropertyName(nameof(IsRunning))]
        private bool _isRunning;

        [ContractPublicPropertyName(nameof(Progress))]
        private double _progress;

        [ContractPublicPropertyName(nameof(CanExecute))]
        private bool _canExecute = true;
    }
}
