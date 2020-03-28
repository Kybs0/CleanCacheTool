using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为 <see cref="AsyncCommand"/> 的异步命令执行提供命令参数和命令状态的修改。
    /// </summary>
    public class AsyncExecutingContext
    {
        /// <summary>
        /// 为命令的实现方提供控制异步任务进度的方法。
        /// </summary>
        [NotNull]
        private readonly IAsyncProgress _asyncProgress;

        /// <summary>
        /// 创建 <see cref="AsyncExecutingContext"/> 的新实例。并同时指定命令的参数。
        /// </summary>
        /// <param name="asyncCommand">异步命令的进度控制。</param>
        internal AsyncExecutingContext([NotNull] IAsyncProgress asyncCommand)
        {
            _asyncProgress = asyncCommand ?? throw new ArgumentNullException(nameof(asyncCommand));
        }

        /// <summary>
        /// 为异步任务报告进度百分比，取值范围为 [0, 1]。
        /// </summary>
        /// <param name="progress">进度百分比，取值范围为 [0, 1]。</param>
        public void ReportProgress(double progress)
        {
            if (progress < 0 || progress > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(progress), "Progress can only be in range of 0 to 1.");
            }
            _asyncProgress.ReportProgress(progress);
        }
    }
}
