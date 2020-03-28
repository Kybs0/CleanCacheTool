using System.Collections.Generic;
using System.Windows;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 专为原生实现的 <see cref="ViewModelProvider"/> 提供扩展方法。
    /// </summary>
    internal static class InternalViewModelProviderExtensions
    {
        /// <summary>
        /// 在过滤并解弱引用后的 <see cref="ViewModelProvider._managedViewViewModelSets"/> 集合中查找活动的 <see cref="FrameworkElement"/>。
        /// 活动的是指当前正在显示的；如果找不到，则查找最后一个（意味着最近使用的）；如果仍找不到（说明一个也没创建或全被回收），则返回 null。
        /// </summary>
        /// <remarks>
        /// 适用于从此 View 发起操作，例如从此 View 导航，从此 View 关闭。通常，这意味着此 View 当前一定处于活动状态。
        /// 如果 View 是被操纵的目标，请不要使用此方法，因为被操作的 View 可能不存在，可能存在但被隐藏。
        /// </remarks>
        [CanBeNull]
        internal static FrameworkElement ActiveOrLastOrDefaultFrameworkElement(
            [NotNull] this IEnumerable<(FrameworkElement, object)> source,
            FrameworkElementTracker tracker)
        {
            FrameworkElement item = null;
            FrameworkElement activeItem = null;
            foreach (var (v, vm) in source)
            {
                item = v;
                if (Equals(v.DataContext, vm) && v.IsVisible)
                {
                    activeItem = v;
                }
            }

            return activeItem ?? item;
        }

        /// <summary>
        /// 在过滤并解弱引用后的 <see cref="ViewModelProvider._managedViewViewModelSets"/> 集合中寻找当前处于 Loaded 状态的 <see cref="FrameworkElement"/>。
        /// Loaded 是指当前已加入视觉树中，可见或不可见的。
        /// </summary>
        /// <remarks>
        /// 适用于被操作的 View，这个 View 可能此时并没有显示，而是即将显示或被交互。
        /// 如果窗口已关闭，<see cref="FrameworkElement"/> 会被移除视觉树，则 IsLoaded 为 false。
        /// </remarks>
        internal static FrameworkElement LastLoadedFrameworkElement(
            this IEnumerable<(FrameworkElement, object)> source,
            FrameworkElementTracker tracker)
        {
            FrameworkElement activeItem = null;
            foreach (var (v, vm) in source)
            {
                if (Equals(v.DataContext, vm) && tracker.IsOnVisualTree(v))
                {
                    activeItem = v;
                }
            }

            return activeItem;
        }
    }
}
