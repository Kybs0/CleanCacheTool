using System;
using System.Windows;
using System.Windows.Media;

namespace Cvte.Escort.Utils
{
    /// <summary>
    /// 包含可视化树的扩展方法。
    /// </summary>
    internal static class VisualTreeExtensions
    {
        /// <summary>
        /// 从 <paramref name="source"/> 开始沿着父级寻找指定类型的元素，并在找到第一个匹配的类型时执行指定的方法。
        /// </summary>
        internal static void FindAncestorToExecute<T1, T2>(this Visual source,
            Action<T1> action1, Action<T2> action2)
        {
            object sourceObject = source;
            while (sourceObject != null)
            {
                if (sourceObject is T1 t1)
                {
                    action1(t1);
                    return;
                }

                if (sourceObject is T2 t2)
                {
                    action2(t2);
                    return;
                }

                sourceObject = VisualTreeHelper.GetParent((DependencyObject) sourceObject);
            }
        }

        /// <summary>
        /// 从 <paramref name="source"/> 开始沿着父级寻找指定类型的元素，并在找到第一个匹配的类型时执行指定的方法。
        /// </summary>
        internal static void FindAncestorToExecute<T1, T2, T3>(this Visual source,
            Action<T1> action1, Action<T2> action2, Action<T3> action3)
        {
            object sourceObject = source;
            while (sourceObject != null)
            {
                if (sourceObject is T1 t1)
                {
                    action1(t1);
                    break;
                }

                if (sourceObject is T2 t2)
                {
                    action2(t2);
                    break;
                }

                if (sourceObject is T3 t3)
                {
                    action3(t3);
                    break;
                }

                sourceObject = VisualTreeHelper.GetParent((DependencyObject) sourceObject);
            }
        }
    }
}
