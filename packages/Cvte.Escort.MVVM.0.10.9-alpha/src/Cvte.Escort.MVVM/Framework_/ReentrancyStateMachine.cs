using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 表示一个异步任务状态机，用于在不同的重新进入策略下决定何时执行指定任务。
    /// </summary>
    public abstract class ReentrancyStateMachine
    {
        /// <summary>
        /// 在派生类中调用此委托以真实地执行异步任务。
        /// </summary>
        [NotNull]
        protected Func<object, Task> AsyncAction { get; private set; }

        /// <summary>
        /// 尝试执行进行一次异步任务，但真实的是否执行的逻辑和执行实际将由派生类实现的重新进入策略决定。
        /// </summary>
        /// <param name="parameter">异步任务中传入的参数。</param>
        /// <returns></returns>
        public abstract Task Invoke(object parameter = null);

        /// <summary>
        /// 如果异步任务正在执行，则尝试取消；否则不做任何事情。
        /// </summary>
        internal abstract void Cancel();

        /// <summary>
        /// 使用简单工厂方法创建一个用于执行 <paramref name="asyncAction"/> 异步任务的重新进入状态机。
        /// </summary>
        /// <param name="asyncAction">要被异步重新进入状态机调用的具体异步任务。</param>
        /// <param name="reentrancyPolicy">重新进入策略。</param>
        public static ReentrancyStateMachine FromPolicy(Func<object, Task> asyncAction,
            ReentrancyPolicy reentrancyPolicy)
        {
            ReentrancyStateMachine stateMachine;
            switch (reentrancyPolicy)
            {
                case ReentrancyPolicy.Disable:
                    stateMachine = new DisableReentrancyStateMachine();
                    break;
                case ReentrancyPolicy.RunImmediately:
                    stateMachine = new RunImmediatelyReentrancyStateMachine();
                    break;
                //case ReentrancyPolicy.CancelAndRestart:
                //    stateMachine = new CancelAndRestartReentrancyStateMachine();
                //    break;
                case ReentrancyPolicy.Queue:
                    stateMachine = new QueueReentrancyStateMachine();
                    break;
                case ReentrancyPolicy.KeepLast:
                    stateMachine = new KeepLastReentrancyStateMachine();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reentrancyPolicy), reentrancyPolicy, null);
            }

            stateMachine.AsyncAction = asyncAction ?? throw new ArgumentNullException(nameof(asyncAction));
            return stateMachine;
        }
    }

    internal sealed class DisableReentrancyStateMachine : ReentrancyStateMachine
    {
        private Task _currentTask;

        public override async Task Invoke(object parameter = null)
        {
            if (_currentTask != null)
            {
                return;
            }

            _currentTask = AsyncAction(parameter);
            try
            {
                await _currentTask.ConfigureAwait(false);
            }
            finally
            {
                _currentTask = null;
            }
        }

        internal override void Cancel()
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class RunImmediatelyReentrancyStateMachine : ReentrancyStateMachine
    {
        public override Task Invoke(object parameter = null)
        {
            return AsyncAction(parameter);
        }

        internal override void Cancel()
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class CancelAndRestartReentrancyStateMachine : ReentrancyStateMachine
    {
        public override Task Invoke(object parameter = null)
        {
            throw new NotImplementedException();
        }

        internal override void Cancel()
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class QueueReentrancyStateMachine : ReentrancyStateMachine
    {
        /// <summary>
        /// 获取此时此刻正在执行的异步任务的 <see cref="Task"/>。
        /// </summary>
        private Task _runningTask;

        /// <summary>
        /// 如果当前没有任务在执行，则立即执行异步任务，并返回可等待此异步任务完成的 <see cref="Task"/>。
        /// 如果当前已有异步任务执行中，则将新的异步任务放入此前任务结束后执行的计划当中，并在返回可等待计划中的新异步任务完成的 <see cref="Task"/>。
        /// </summary>
        public override async Task Invoke(object parameter = null)
        {
            // 在异步任务开始之前，先记录并验证当前任务。
            var lastTask = _runningTask;

            // 如果异步任务尚未开始，则立即开始一个，否则在当前任务后补充一个。
            Task currentTask;
            if (lastTask == null)
            {
                currentTask = AsyncAction(parameter);
            }
            else
            {
                currentTask = await lastTask
                    .ContinueWith(async task => await AsyncAction(parameter).ConfigureAwait(false),
                        SynchronizationContext.Current == null
                            ? TaskScheduler.Current
                            : TaskScheduler.FromCurrentSynchronizationContext())
                    .ConfigureAwait(false);
            }

            // 现在，当前任务就变成最新的那个了（即便尚未开始，但如果后续还有任务入队执行，则会接着这时设置的任务）。
            _runningTask = currentTask;

            // 每个入队的任务都只等待自己这个任务的执行（可能被 Task.ContinueWith 包装，以便延后执行。）
            await currentTask.ConfigureAwait(false);

            _runningTask = lastTask;
        }

        internal override void Cancel()
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class KeepLastReentrancyStateMachine : ReentrancyStateMachine
    {
        private readonly object _locker = new object();

        private readonly ConcurrentStack<object> _paramaterStack = new ConcurrentStack<object>();

        private bool _isRunning = false;

        //正在运行的任务参数
        private object _runningTaskParamater;

        //包装一个任务结果，用于实现等待控制
        private TaskCompletionSource<bool> _runningSource = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _nextSource = new TaskCompletionSource<bool>();

        /// <summary>
        /// 若当前没有任务，则立刻执行，否则将最后一个任务置为下一个任务
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public override Task Invoke(object parameter = null)
        {
            _paramaterStack.Push(parameter);
            lock (_locker)
            {
                if (!_isRunning)
                {
                    ExecuteCoreAsync();
                    return _runningSource.Task;
                }
                else
                {
                    return _nextSource.Task;
                }
            }
        }

        ///实际执行方式
        private async void ExecuteCoreAsync()
        {
            lock (_locker)
            {
                if (_paramaterStack.IsEmpty)
                {
                    _isRunning = false;
                    _runningSource.SetResult(true);
                    _runningSource = _nextSource;
                    _nextSource = new TaskCompletionSource<bool>();
                    return;
                }

                _paramaterStack.TryPop(out _runningTaskParamater);
                _paramaterStack.Clear();
            }


            lock (_locker)
            {
                _isRunning = true;
            }

            await AsyncAction(_runningTaskParamater).ContinueWith(async task => await ContinueTask());
        }

        private Task ContinueTask()
        {
            lock (_locker)
            {
                _runningSource.SetResult(true);
                _runningSource = _nextSource;
                _nextSource = new TaskCompletionSource<bool>();
                if (!_paramaterStack.IsEmpty)
                {
                    _paramaterStack.TryPop(out _runningTaskParamater);
                    _paramaterStack.Clear();
                    return AsyncAction(_runningTaskParamater).ContinueWith(async task => await ContinueTask());
                }
            }

            _isRunning = false;
            return new TaskCompletionSource<bool>(true).Task;
        }

        internal override void Cancel()
        {
            throw new NotImplementedException();
        }
    }
}
