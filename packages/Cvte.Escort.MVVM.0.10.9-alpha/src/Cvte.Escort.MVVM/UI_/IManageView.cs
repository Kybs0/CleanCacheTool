using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 表示一个 View 将接管 <see cref="IViewModelProvider"/> 对某种 ViewModel 所关联的 View 显示过程的自动控制。
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public interface IManageView<in TViewModel> where TViewModel : IViewModel
    {
        /// <summary>
        /// 显示一个 ViewModel 所关联的 View。
        /// </summary>
        /// <param name="viewModel">ViewModel 的实例。</param>
        void ShowView([NotNull] TViewModel viewModel);
    }
}
