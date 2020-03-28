namespace Cvte.Escort
{
    /// <summary>
    /// <para>表示异步任务执行时的重新进入策略。</para>
    /// <remarks>
    /// 有关重新进入的详细信息，请参阅：[Handling Reentrancy in Async Apps (C#) - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/handling-reentrancy-in-async-apps)
    /// </remarks>
    /// </summary>
    public enum ReentrancyPolicy
    {
        /// <summary>
        /// <para>当一次异步任务开始时，会禁用此异步任务的后续执行，直到此次任务执行完毕。</para>
        /// <remarks>
        /// 这是默认值。
        /// </remarks>
        /// </summary>
        Disable,

        /// <summary>
        /// 每当异步任务需要执行，无论当前是否已经开始，都会立即开始执行一个新的任务。
        /// <remarks>
        /// 一般不建议采用此默认值，除非明确知道并发可能产生的副作用。
        /// </remarks>
        /// </summary>
        RunImmediately,

        ///// <summary>
        ///// <para>当新的异步任务开始执行时，如果当前有一个正在执行的异步任务，则会尝试取消当前任务，然后开始新的任务。</para>
        ///// <para>需要注意：</para>
        ///// <para>1. 如果任务本身并不支持取消，则会等待任务完成；</para>
        ///// <para>2. 如果试图更多次执行，则只有第一次执行和最后一次执行会真正开始，其中第一次如果支持取消则会尝试取消。</para>
        ///// <remarks>
        ///// 例如网络请求，用户可能因为第一次请求正在进行超时等待而重新发起一次请求；这时取消前一次的请求（如果支持）而发起一项新的。
        ///// </remarks>
        ///// </summary>
        //CancelAndRestart,

        /// <summary>
        /// <para>每个新的异步任务都会加入到任务执行队列并排队依次执行。</para>
        /// <remarks>
        /// 如果每次执行都会产生不一样的状态，则推荐使用此策略。例如增加数据。
        /// </remarks>
        /// </summary>
        Queue,

        /// <summary>
        /// <para>
        /// 当新的异步任务开始执行时，如果当前有一个正在执行的异步任务，则会记录任务再次执行；
        /// 但无论重新开始多少次，都只会在当前任务结束后执行一次。
        /// </para>
        /// <remarks>
        /// 如果状态相同时执行任务的结果也相同，则推荐使用此策略。例如保存文件，只要能够保存新的文件即可，旧的版本不需要保存。
        /// </remarks>
        /// </summary>
        KeepLast,
    }
}
