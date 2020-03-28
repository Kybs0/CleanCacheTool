using System;
using System.Collections.Generic;
using System.Linq;

namespace Cvte.Escort
{
    /// <summary>
    /// 包含弱引用序列的扩展方法。
    /// </summary>
    internal static class WeakReferenceEnumerableExtensions
    {
        /// <summary>
        /// 对弱引用二元元组进行遍历，并挑选符合条件的元素。每遍历发现一个已被垃圾回收的元组，就会将其从集合中删除。
        /// </summary>
        /// <typeparam name="T1">弱引用元组类型 1。</typeparam>
        /// <typeparam name="T2">弱引用元组类型 2。</typeparam>
        /// <param name="source">弱引用元组集合。注意，此遍历可能会修改此集合。</param>
        /// <param name="predicate">挑选符合条件的元素的筛选条件。</param>
        /// <returns>筛选出来的元素序列。</returns>
        public static IEnumerable<(T1, T2)> Where<T1, T2>(
            this ICollection<(WeakReference<T1>, WeakReference<T2>)> source,
            Func<T1, T2, bool> predicate)
            where T1 : class where T2 : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            // 遍历弱引用元组，以便找到已缓存的 View 和 ViewModel 元组。
            foreach (var (weakT1, weakT2) in source.ToList())
            {
                if (weakT1.TryGetTarget(out var t1) && weakT2.TryGetTarget(out var t2))
                {
                    var match = predicate(t1, t2);
                    if (match)
                    {
                        yield return (t1, t2);
                    }
                }
                else
                {
                    source.Remove((weakT1, weakT2));
                }
            }
        }
    }
}
