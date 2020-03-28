using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace Cvte.Escort
{
    /// <summary>
    /// 为 <see cref="IViewModel"/> 提供 View 与 ViewModel 的管理。
    /// </summary>
    public interface IViewModelProvider
    {
        /// <summary>
        /// 从参数 <paramref name="viewModelSource"/> 指定的 ViewModel 中显示一个新的 ViewModel，
        /// 这个新 ViewModel 的类型是 <typeparamref name="TViewModel"/>。
        /// </summary>
        /// <typeparam name="TViewModel">要显示新 View 的 ViewModel。</typeparam>
        /// <param name="viewModelSource">
        /// 如果需要显示新的 View，需要有一个用于承载此新 View 的载体 View；
        /// 此参数是此 View 对应的 ViewModel。
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        void ShowViewFrom<TViewModel>(IViewModel viewModelSource);

        /// <summary>
        /// 关闭关联此 <paramref name="viewModelSource"/> 的 View。这将关闭窗口或者导航返回 Page。
        /// </summary>
        /// <param name="viewModelSource">要关闭的 View 对应的 ViewModel。</param>
        void CloseViewFrom(IViewModel viewModelSource);

#if NETFRAMEWORK

        /// <summary>
        /// 获取或创建与 <paramref name="viewAsKey"/> 对应的指定类型的 ViewModel。
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel 的类型。可以是接口。</typeparam>
        /// <param name="viewAsKey">此 ViewModel 将关联的 View。</param>
        /// <returns></returns>
        TViewModel Resolve<TViewModel>(FrameworkElement viewAsKey);

#endif

        /// <summary>
        /// 为了和 Cvte.Composition.Container 交接类型，需要能够从依赖反转容器中取得某个 ViewModel 的实例。
        /// </summary>
        [Obsolete("用于和 Cvte.Composition.Container 交接。")]
        TViewModel GetViewModel<TViewModel>();
    }
}
