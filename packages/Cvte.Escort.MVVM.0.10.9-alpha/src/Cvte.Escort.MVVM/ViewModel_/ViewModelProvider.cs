using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Cvte.Escort.Annotations;
using Cvte.Escort.DI;

#if NETFRAMEWORK
using System.Windows;
using Cvte.Escort.Utils;
// 由于 (WeakReference<FrameworkElement>, WeakReference<ViewModelBase>) 太长，
// 多次出现时会因为与其他元组的细微差异造成阅读困难，所以使用一个较短的名称代替。
using WeakViewViewModelTuple = System.ValueTuple<System.WeakReference<System.Windows.FrameworkElement>, System.WeakReference<object>>;
#endif

namespace Cvte.Escort
{
    /// <inheritdoc />
    /// <summary>
    /// 为 View 和 ViewModel 之间的关联提供默认的管理方式。
    /// </summary>
    public class ViewModelProvider : IViewModelProvider
    {
#region Static Members

        private static IViewModelProvider _current = new ViewModelProvider(
            Enumerable.Empty<IContractInfo>(), Enumerable.Empty<IContractInfo>());

#if NETFRAMEWORK
        /// <summary>
        /// 获取当前正在使用的 <see cref="IViewModelProvider"/> 的实例，
        /// 用于给 <see cref="ViewModelExtension"/> 标记扩展提供 View/ViewModel 的关联依据。
        /// </summary>
#else
        /// <summary>
        /// 获取当前正在使用的 <see cref="IViewModelProvider"/> 的实例，默认为 null。
        /// </summary>
#endif
        public static IViewModelProvider Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

#endregion

#if NETFRAMEWORK

#region Private Fields
        
        private readonly CompositionHost _viewModelHost;
        private readonly CompositionHost _viewHost;
        private readonly FrameworkElementTracker _tracker = new FrameworkElementTracker();

        private readonly HashSet<WeakViewViewModelTuple> _managedViewViewModelSets =
            new HashSet<WeakViewViewModelTuple>();

#endregion

#region Constructors

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1707 // Identifiers should not contain underscores

        private const string ObsoleteReasonOfPerformance = "由于性能问题，在正式版发布之后，此构造函数将删除。";

        [Obsolete(ObsoleteReasonOfPerformance, true)]
        public ViewModelProvider() => throw new NotSupportedException(ObsoleteReasonOfPerformance);

        [Obsolete(ObsoleteReasonOfPerformance, true)]
        public ViewModelProvider(Assembly _) => throw new NotSupportedException(ObsoleteReasonOfPerformance);

        [Obsolete(ObsoleteReasonOfPerformance, true)]
        public ViewModelProvider(IEnumerable<Assembly> _)
            => throw new NotSupportedException(ObsoleteReasonOfPerformance);

        [Obsolete(ObsoleteReasonOfPerformance, true)]
        public ViewModelProvider(IEnumerable<Assembly> vm, IEnumerable<Assembly> v, bool vmi = false, bool vi = false)
            => throw new NotSupportedException(ObsoleteReasonOfPerformance);

        [Obsolete(ObsoleteReasonOfPerformance, true)]
        public ViewModelProvider(IEnumerable<Type> _) => throw new NotSupportedException(ObsoleteReasonOfPerformance);

        [Obsolete(ObsoleteReasonOfPerformance, true)]
        public ViewModelProvider(IEnumerable<Type> vm, IEnumerable<Type> v)
            => throw new NotSupportedException(ObsoleteReasonOfPerformance);

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CA1707 // Identifiers should not contain underscores

        /// <summary>
        /// 创建新的 View/ViewModel 管理方式。对于大型程序集，直接指定导出的类型比指定导出程序集具有更高的性能。
        /// <para>
        /// 其中 <paramref name="viewModelContracts"/> 为所有应该导出的 ViewModel
        /// <paramref name="viewContracts"/> 为所有应该导出的 View。
        /// </para>
        /// </summary>
        /// <param name="viewModelContracts">应该导出的所有 ViewModel 的导出类型和对应的创建方法。</param>
        /// <param name="viewContracts">应该导出的所有 View 的导出类型和对应的创建方法。</param>
        public ViewModelProvider(
            [NotNull] IEnumerable<IContractInfo> viewModelContracts,
            [NotNull] IEnumerable<IContractInfo> viewContracts)
        {
            if (viewModelContracts == null) throw new ArgumentNullException(nameof(viewModelContracts));
            if (viewContracts == null) throw new ArgumentNullException(nameof(viewContracts));

            _viewModelHost = new CompositionHost(viewModelContracts);
            _viewHost = new CompositionHost(viewContracts, false);
        }

#endregion

#region 依赖发现与注入

        /// <summary>
        /// 额外添加一些 View/ViewModel 的依赖信息。对于大型程序集，直接指定导出的类型比指定导出程序集具有更高的性能。
        /// <para>
        /// 其中 <paramref name="viewModelContracts"/> 为所有应该导出的 ViewModel
        /// <paramref name="viewContracts"/> 为所有应该导出的 View。
        /// </para>
        /// </summary>
        /// <param name="viewModelContracts">应该导出的所有 ViewModel 的导出类型和对应的创建方法。</param>
        /// <param name="viewContracts">应该导出的所有 View 的导出类型和对应的创建方法。</param>
        [PublicAPI]
        public void AppendContracts(
            [NotNull] IEnumerable<IContractInfo> viewModelContracts,
            [NotNull] IEnumerable<IContractInfo> viewContracts)
        {
            if (viewModelContracts == null) throw new ArgumentNullException(nameof(viewModelContracts));
            if (viewContracts == null) throw new ArgumentNullException(nameof(viewContracts));

            _viewModelHost.Append(viewModelContracts);
            _viewHost.Append(viewContracts);
        }

        /// <inheritdoc />
        [NotNull, Obsolete("用于和 Cvte.Composition.Container 交接。")]
        public TViewModel GetViewModel<TViewModel>()
        {
            var viewModel = _viewModelHost.GetExport<TViewModel>();
            _viewModelHost.SatisfyImports(viewModel);
            return viewModel;
        }

        /// <summary>
        /// 获取或创建与 <paramref name="viewAsKey" /> 对应的指定类型的 ViewModel。
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel 的类型。可以是接口。</typeparam>
        /// <param name="viewAsKey">此 ViewModel 将关联的 View。</param>
        /// <returns></returns>
        [NotNull]
        public TViewModel Resolve<TViewModel>([NotNull] FrameworkElement viewAsKey)
        {
            if (viewAsKey == null)
                throw new ArgumentNullException(nameof(viewAsKey));

            // 尝试从 View-ViewModel 管理集合中寻找 ViewModel。
            var (_, viewModel) = _managedViewViewModelSets
                .Where((v, _) => Equals(viewAsKey, v)).FirstOrDefault();
            if (viewModel != null)
            {
                return (TViewModel) viewModel;
            }

            // 以下开始处理缓存中未找到时新建 ViewModel 的情况。
            viewModel = ResolveViewModelCore<TViewModel>(viewAsKey);
            if (viewModel is ViewModelBase vm)
            {
                this.Connect(vm);
                this.Attach(vm);
            }

            viewAsKey.Loaded += View_Loaded;
            viewAsKey.Unloaded += View_Unloaded;
            _managedViewViewModelSets.Add(
                (new WeakReference<FrameworkElement>(viewAsKey),
                new WeakReference<object>(viewModel)));
            return (TViewModel) viewModel;
        }

        /// <summary>
        /// 在派生类中重写此方法时，指定如何创建一个 ViewModel 并关联到 View 中。
        /// </summary>
        /// <param name="view">需要关联的 View。</param>
        /// <typeparam name="TViewModel">需要创建的 ViewModel 的契约类型（可能是接口）。</typeparam>
        /// <returns>获取到的 ViewModel 的实例。</returns>
        [NotNull]
        protected virtual TViewModel ResolveViewModelCore<TViewModel>([NotNull] FrameworkElement view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var viewModel = _viewModelHost.GetExport<TViewModel>();
            _viewModelHost.SatisfyImports(viewModel);
            _viewHost.SatisfyImports(view, _viewModelHost);
            return viewModel;
        }

#endregion

#region View 与 ViewModel 之间的导航

        /// <inheritdoc />
        /// <summary>
        /// 从 <paramref name="viewModelSource" /> 中查找一个关联了 <typeparamref name="TViewModel" /> 的 View，并将其显示出来。
        /// </summary>
        /// <param name="viewModelSource">开始查找的 ViewModel。如果是从 <see cref="Cvte.Escort.ViewModelBase" />（基类而非子类）中调用，则传入自身。</param>
        /// <typeparam name="TViewModel">要显示的关联到 ViewModel 的 View。</typeparam>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void ShowViewFrom<TViewModel>([NotNull] IViewModel viewModelSource)
        {
            if (viewModelSource == null)
                throw new ArgumentNullException(nameof(viewModelSource));

            var type = viewModelSource.GetType();
            var view = _managedViewViewModelSets
                .Where((v, vm) => vm.GetType().IsAssignableFrom(type))
                .ActiveOrLastOrDefaultFrameworkElement(_tracker);
            if (view != null)
            {
                // 一般来说都会进入此分支。因为能够通过 ViewModel 执行的代码，View 也应该存在。
                // 而这里，会将此 View 作为控制方，显示一个新 View，而新 View 的 ViewModel 是 TViewModel。
                ShowViewFrom<TViewModel>(view);
            }
        }

        private void ShowViewFrom<TViewModel>(FrameworkElement view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var subView = _managedViewViewModelSets
                .Where((v, vm) => vm is TViewModel)
                .LastLoadedFrameworkElement(_tracker);
            subView = subView ?? CreateViewFromViewModel<TViewModel>();
            view.FindAncestorToExecute<ViewFrame, Window>(
                frame => { frame.Navigate(subView); },
                window => { }
            );
        }

        /// <summary>
        /// 从 <paramref name="viewModelSource" /> 所关联的 View 中查找最接近的可以关闭的 View，并将其关闭。
        /// </summary>
        /// <param name="viewModelSource"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void CloseViewFrom([NotNull] IViewModel viewModelSource)
        {
            if (viewModelSource == null)
                throw new ArgumentNullException(nameof(viewModelSource));

            var type = viewModelSource.GetType();
            var view = _managedViewViewModelSets
                .Where((v, vm) => vm.GetType().IsAssignableFrom(type))
                .ActiveOrLastOrDefaultFrameworkElement(_tracker);
            if (view != null)
            {
                // 一般来说，能够通过 ViewModel 执行的代码，View 也应该存在。
                CloseViewFrom(view);
            }
        }

        private static void CloseViewFrom([NotNull] FrameworkElement view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            view.FindAncestorToExecute<IClosableView, ViewFrame, Window>(
                closable => closable.CloseView(),
                frame => frame.GoBack(),
                window => window.Close()
            );
        }

        [NotNull]
        private FrameworkElement CreateViewFromViewModel<TViewModel>()
        {
            var host = _viewHost;
            var element = (FrameworkElement) host.GetExport(typeof(TViewModel));
            _tracker.Track(element);
            return element;
        }

        /// <summary>
        /// 当 View Loaded 的时候触发。这是事件处理函数，而且还是静态的，反正也无法被垃圾回收，那就不考虑事件的 -= 了。
        /// </summary>
        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ViewModelBase vm)
            {
                this.Load(vm);
            }
        }

        /// <summary>
        /// 当 View Unloaded 的时候触发。这是事件处理函数，而且还是静态的，反正也无法被垃圾回收，那就不考虑事件的 -= 了。
        /// </summary>
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ViewModelBase vm)
            {
                this.Unload(vm);
            }
        }

#endregion

#region View 与 ViewModel 之间的消息传递

        /// <summary>
        /// 未实现。
        /// </summary>
        /// <param name="viewModelSource"></param>
        /// <param name="message"></param>
        /// <typeparam name="TMessage"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public void SendMessageFrom<TMessage>(IViewModel viewModelSource, TMessage message) where TMessage : IMessage
        {
            if (viewModelSource == null)
                throw new ArgumentNullException(nameof(viewModelSource));

            var type = viewModelSource.GetType();
            var view = _managedViewViewModelSets
                .Where((v, vm) => vm.GetType().IsAssignableFrom(type))
                .ActiveOrLastOrDefaultFrameworkElement(_tracker);
            if (view != null)
            {
                // 一般来说，能够通过 ViewModel 执行的代码，View 也应该存在。
                SendMessageFrom(view, message);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "message")]
        private void SendMessageFrom<TMessage>(FrameworkElement view, TMessage message) where TMessage : IMessage
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            throw new NotImplementedException();
        }

#endregion

#else
        private const string NetStandardNotSupportedTip = "暂时不支持跨平台版本的 IViewModelProvider 的默认实现。";

        /// <inheritdoc />
        private ViewModelProvider(IEnumerable<IContractInfo> vm, IEnumerable<IContractInfo> v)
            => throw new PlatformNotSupportedException(NetStandardNotSupportedTip);

        /// <inheritdoc />
        public void ShowViewFrom<TViewModel>(IViewModel viewModelSource)
            => throw new PlatformNotSupportedException(NetStandardNotSupportedTip);

        /// <inheritdoc />
        public void CloseViewFrom(IViewModel viewModelSource)
            => throw new PlatformNotSupportedException(NetStandardNotSupportedTip);

        /// <inheritdoc />
        public TViewModel GetViewModel<TViewModel>()
            => throw new PlatformNotSupportedException(NetStandardNotSupportedTip);
#endif
    }
}
