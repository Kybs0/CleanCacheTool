using System;
using Cvte.Escort.Annotations;
#if NETFRAMEWORK
using TNavigation = System.Windows.UIElement;
#else
using TNavigation = System.Object;
#endif

namespace Cvte.Escort
{
    /// <summary>
    /// 在页面导航时传递的参数。
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// 创建包含基本导航信息的导航参数。
        /// </summary>
        /// <param name="source">导航的来源页。</param>
        /// <param name="target">导航的目标页。</param>
        /// <param name="parameter">导航中传递的参数，可以为 null。</param>
        public NavigationEventArgs([CanBeNull] TNavigation source, [CanBeNull] TNavigation target,
            object parameter = null)
        {
            Source = source;
            Target = target;
            Parameter = parameter;
        }

        /// <summary>
        /// 导航的来源页，null 表示没有来源页。
        /// </summary>
        [CanBeNull]
        public TNavigation Source { get; }

        /// <summary>
        /// 导航的目标页，null 表示没有目标页。
        /// </summary>
        [CanBeNull]
        public TNavigation Target { get; }

        /// <summary>
        /// 导航中传递的参数，null 表示传递参数。
        /// </summary>
        [CanBeNull]
        public object Parameter { get; }
    }

    /// <summary>
    /// 在页面导航时传递的参数，可通过此参数取消导航。
    /// </summary>
    public class NavigatingCancelEventArgs : NavigationEventArgs
    {
        /// <summary>
        /// 创建包含基本导航信息的导航参数，可通过此参数取消导航。
        /// </summary>
        /// <param name="source">导航的来源页。</param>
        /// <param name="target">导航的目标页。</param>
        /// <param name="parameter">导航中传递的参数，可以为 null。</param>
        public NavigatingCancelEventArgs([CanBeNull] TNavigation source, [CanBeNull] TNavigation target,
            [CanBeNull] object parameter = null)
            : base(source, target, parameter)
        {
        }

        /// <summary>
        /// 获取是否有任何导航过程取消了此次导航。
        /// </summary>
        public bool IsCanceled { get; private set; }

        /// <summary>
        /// 取消一次导航。
        /// </summary>
        public void Cancel()
        {
            IsCanceled = true;
        }
    }

    /// <summary>
    /// 包含导航参数的扩展方法。
    /// </summary>
    public static class NavigationEventArgsExtensions
    {
        /// <summary>
        /// 从导航参数中获取指定类型的参数。
        /// </summary>
        public static T GetParameter<T>([NotNull] this NavigationEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.Parameter is null)
            {
                return default;
            }
            return (T) args.Parameter;
        }
    }
}
