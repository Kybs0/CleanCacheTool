namespace Cvte.Escort
{
    /// <summary>
    /// 表示此异步任务支持报告进度。
    /// </summary>
    internal interface IAsyncProgress
    {
        /// <summary>
        /// 为异步任务报告进度百分比，取值范围为 [0, 1]。
        /// </summary>
        /// <param name="progress">进度百分比，取值范围为 [0, 1]。</param>
        void ReportProgress(double progress);
    }
}
