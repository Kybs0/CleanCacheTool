using System;
using System.Threading.Tasks;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为异步任务提供 <see cref="System.Windows.Input.ICommand"/>。
    /// </summary>
    public sealed class AsyncCommand : AsyncCommandBase
    {
        /// <summary>
        /// 创建用于执行指定异步任务 <paramref name="asyncAction"/> 的 <see cref="AsyncCommand"/>。
        /// 可选的，可以指定异步任务的重新进入策略。
        /// </summary>
        /// <param name="asyncAction">异步命令中执行的异步任务。</param>
        /// <param name="reentrancyPolicy">异步任务的重新进入策略。</param>
        public AsyncCommand([NotNull] Func<Task> asyncAction,
            ReentrancyPolicy reentrancyPolicy = default)
            : base(reentrancyPolicy)
        {
            if (asyncAction == null)
                throw new ArgumentNullException(nameof(asyncAction));

            _asyncAction = context => asyncAction();
        }

        /// <summary>
        /// 创建用于执行指定异步任务 <paramref name="asyncAction"/> 的 <see cref="AsyncCommand"/>。
        /// 可选的，可以指定异步任务的重新进入策略。
        /// </summary>
        /// <param name="asyncAction">异步命令中执行的异步任务。</param>
        /// <param name="reentrancyPolicy">异步任务的重新进入策略。</param>
        public AsyncCommand([NotNull] Func<AsyncExecutingContext, Task> asyncAction,
            ReentrancyPolicy reentrancyPolicy = default)
            : base(reentrancyPolicy)
        {
            _asyncAction = asyncAction ?? throw new ArgumentNullException(nameof(asyncAction));
        }

        /// <summary>
        /// 异步任务。执行此委托将执行具体的异步任务。
        /// </summary>
        [NotNull]
        private readonly Func<AsyncExecutingContext, Task> _asyncAction;

        /// <inheritdoc />
        /// <summary>
        /// 派生类重写此方法时，实际执行异步任务。
        /// </summary>
        /// <param name="parameter">由 <see cref="System.Windows.Input.ICommand" /> 接口传入的参数。</param>
        protected override Task ExecuteAsyncCore(object parameter)
        {
            return _asyncAction(new AsyncExecutingContext(this));
        }

        /// <summary>
        /// 执行此命令指定的异步任务。
        /// 如果任务已经在执行，则根据 <see cref="ReentrancyPolicy"/> 指定的重新进入策略重新进入。
        /// </summary>
        [NotNull]
        public Task ExecuteAsync()
        {
            return ExecuteAsync(null);
        }
    }
}
