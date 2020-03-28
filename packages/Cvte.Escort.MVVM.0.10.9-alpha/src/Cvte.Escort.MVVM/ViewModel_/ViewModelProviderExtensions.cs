using System;
using System.Diagnostics.CodeAnalysis;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为 <see cref="IViewModelProvider"/> 的完整实现提供辅助方法。
    /// </summary>
    public static class ViewModelProviderExtensions
    {
        /// <summary>
        /// 当类型实现了 <see cref="IViewModelProvider"/> 接口后，通过此扩展方法
        /// 将 <see cref="ViewModelBase"/> 关联到此 <see cref="IViewModelProvider"/>。
        /// <para/>
        /// 这样，<see cref="ViewModelBase"/> 将可以向 <see cref="IViewModelProvider"/>
        /// 发送它与其他 View 和 ViewModel 的通信请求。
        /// </summary>
        /// <param name="provider"><see cref="IViewModelProvider"/> 接口的实例。</param>
        /// <param name="viewModel">被 <paramref name="provider"/> 管理的 <see cref="ViewModelBase"/>。</param>
        /// <exception cref="ArgumentNullException">
        /// 参数 <paramref name="provider"/> 或 <paramref name="viewModel"/> 为 null。
        /// </exception>
        public static void Connect([NotNull] this IViewModelProvider provider,
            [NotNull] ViewModelBase viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            ((IViewModelProviderConnectionTarget) viewModel).Provider =
                provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// 当类型实现了 <see cref="IViewModelProvider"/> 接口后，通过此扩展方法
        /// 通知 <see cref="ViewModelBase"/> 第一个 View 与此 ViewModel 的关联已经开始。
        /// </summary>
        /// <param name="provider"><see cref="IViewModelProvider"/> 接口的实例。</param>
        /// <param name="viewModel">被 <paramref name="provider"/> 管理的 <see cref="ViewModelBase"/>。</param>
        /// <exception cref="ArgumentNullException">
        /// 参数 <paramref name="provider"/> 或 <paramref name="viewModel"/> 为 null。
        /// </exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "provider")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void Attach([NotNull, UsedImplicitly] this IViewModelProvider provider,
            [NotNull] ViewModelBase viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            ((IViewModelProviderConnectionTarget)viewModel).Attach();
        }

        /// <summary>
        /// 当类型实现了 <see cref="IViewModelProvider"/> 接口后，通过此扩展方法
        /// 通知 <see cref="ViewModelBase"/> 有一个 View 开始使用此 ViewModel 显示。
        /// 每有一个新 View 显示都需要调用一次此方法。
        /// </summary>
        /// <param name="provider"><see cref="IViewModelProvider"/> 接口的实例。</param>
        /// <param name="viewModel">被 <paramref name="provider"/> 管理的 <see cref="ViewModelBase"/>。</param>
        /// <exception cref="ArgumentNullException">
        /// 参数 <paramref name="provider"/> 或 <paramref name="viewModel"/> 为 null。
        /// </exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "provider")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void Load([NotNull, UsedImplicitly] this IViewModelProvider provider,
            [NotNull] ViewModelBase viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            ((IViewModelProviderConnectionTarget)viewModel).Load();
        }

        /// <summary>
        /// 当类型实现了 <see cref="IViewModelProvider"/> 接口后，通过此扩展方法
        /// 通知 <see cref="ViewModelBase"/> 有一个 View 关闭显示。
        /// 每有一个新 View 关闭都需要调用一次此方法。
        /// </summary>
        /// <param name="provider"><see cref="IViewModelProvider"/> 接口的实例。</param>
        /// <param name="viewModel">被 <paramref name="provider"/> 管理的 <see cref="ViewModelBase"/>。</param>
        /// <exception cref="ArgumentNullException">
        /// 参数 <paramref name="provider"/> 或 <paramref name="viewModel"/> 为 null。
        /// </exception>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "provider")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void Unload([NotNull, UsedImplicitly] this IViewModelProvider provider,
            [NotNull] ViewModelBase viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
            ((IViewModelProviderConnectionTarget)viewModel).Unload();
        }
    }
}
