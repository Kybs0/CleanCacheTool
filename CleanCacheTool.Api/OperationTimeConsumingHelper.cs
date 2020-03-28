using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCacheTool.Api
{
    /// <summary>
    /// 操作耗时辅助类
    /// </summary>
    public class OperationTimeConsumingHelper
    {

        private const double DefaultRunTimeAtLeast = 600;

        /// <summary>
        /// 以至少时间执行
        /// </summary>
        /// <param name="action">待执行函数</param>
        /// <returns></returns>
        public static async Task RunWithTimeLimitAsync(Action action)
        {
            await RunWithTimeLimitAsync(action, DefaultRunTimeAtLeast);
        }
        public static async Task RunWithTimeLimitAsync(Action action, double runTimeAtLeast)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            if (stopwatch.ElapsedMilliseconds < runTimeAtLeast)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(runTimeAtLeast - stopwatch.ElapsedMilliseconds));
            }
            stopwatch.Stop();
        }

        /// <summary>
        /// 以至少时间执行
        /// </summary>
        /// <typeparam name="TOut"><paramref name="func"/>的执行结果</typeparam>
        /// <param name="func">待执行函数</param>
        /// <returns></returns>
        public static async Task<TOut> RunWithTimeLimitAsync<TOut>(Func<Task<TOut>> func)
        {
            return await RunWithTimeLimitAsync(func, DefaultRunTimeAtLeast);
        }

        /// <summary>
        /// 以至少时间执行
        /// </summary>
        /// <typeparam name="TOut"><paramref name="func"/>的执行结果</typeparam>
        /// <param name="func">待执行函数</param>
        /// <param name="runTimeAtLeast">执行至少耗时</param>
        /// <returns></returns>
        public static async Task<TOut> RunWithTimeLimitAsync<TOut>(Func<Task<TOut>> func, double runTimeAtLeast)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await func.Invoke();
            if (stopwatch.ElapsedMilliseconds < runTimeAtLeast)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(runTimeAtLeast - stopwatch.ElapsedMilliseconds));
            }
            stopwatch.Stop();
            return result;
        }

        /// <summary>
        /// 以至少时间执行
        /// </summary>
        /// <typeparam name="TOut"><paramref name="task"/>的执行结果</typeparam>
        /// <param name="task">执行函数</param>
        /// <returns></returns>
        public static async Task<TOut> RunWithTimeLimitAsync<TOut>(Task<TOut> task)
        {
            return await RunWithTimeLimitAsync(task, DefaultRunTimeAtLeast);
        }

        public static async Task<TOut> RunWithTimeLimitAsync<TOut>(Task<TOut> task, double runTimeAtLeast)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await task;
            if (stopwatch.ElapsedMilliseconds < runTimeAtLeast)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(runTimeAtLeast - stopwatch.ElapsedMilliseconds));
            }
            stopwatch.Stop();
            return result;
        }
    }
}
