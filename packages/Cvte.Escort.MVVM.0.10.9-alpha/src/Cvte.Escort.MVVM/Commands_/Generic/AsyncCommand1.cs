using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cvte.Escort.Annotations;
using Cvte.Escort.Commands;

namespace Cvte.Escort
{
    /// <inheritdoc />
    /// <summary>
    /// 为具有特定类型命令参数的异步任务提供 <see cref="System.Windows.Input.ICommand" />。
    /// </summary>
    public sealed class AsyncCommand<T> : AsyncCommandBase
    {
        /// <summary>
        /// 创建一个没有异步控制流的，带一个或多个泛型参数的异步命令。同时可以指定此异步的重新进入策略。
        /// </summary>
        /// <param name="asyncAction">异步任务。</param>
        /// <param name="reentrancyPolicy">异步任务的重新进入策略。</param>
        public AsyncCommand([NotNull] Func<T, Task> asyncAction,
            ReentrancyPolicy reentrancyPolicy = default)
            : base(reentrancyPolicy)
        {
            if (asyncAction == null)
                throw new ArgumentNullException(nameof(asyncAction));

            _asyncAction = (context, t) => asyncAction(t);
        }

        /// <summary>
        /// 创建一个带有异步流程控制上下文的，带一个或多个泛型参数的异步命令。同时可以指定此异步的重新进入策略。
        /// </summary>
        /// <param name="asyncAction">带有异步流程控制上下文的异步任务。</param>
        /// <param name="reentrancyPolicy">异步任务的重新进入策略。</param>
        public AsyncCommand([NotNull] Func<AsyncExecutingContext, T, Task> asyncAction,
            ReentrancyPolicy reentrancyPolicy = default)
            : base(reentrancyPolicy)
        {
            _asyncAction = asyncAction ?? throw new ArgumentNullException(nameof(asyncAction));
        }

        /// <summary>
        /// 异步任务。
        /// </summary>
        [NotNull]
        private readonly Func<AsyncExecutingContext, T, Task> _asyncAction;

        /// <summary>
        /// 实际执行异步任务。
        /// </summary>
        protected override async Task ExecuteAsyncCore(object parameter)
        {
            var (t, canExecute) = await ParameterExtractor.ExtractAsync<T>(parameter).ConfigureAwait(true);
            if (canExecute)
            {
                await _asyncAction(new AsyncExecutingContext(this), t).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 以给定的参数执行此命令指定的异步任务。
        /// 如果任务已经在执行，则根据 <see cref="ReentrancyPolicy"/> 指定的重新进入策略重新进入。
        /// </summary>
        [NotNull, SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t")]
        public Task ExecuteAsync(T t)
        {
            object value = t;
            return ExecuteAsync(value);
        }
    }
}
