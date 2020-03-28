using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为 ViewModel 提供基类。可以为 ViewModel 子类提供的功能包括抽象 UI 的导航与通知，依赖注入与单元测试支持和异步任务支持。
    /// </summary>
    public abstract class ViewModelBase : BindableObject, IViewModel, IViewModelProviderConnectionTarget
    {
        /// <summary>
        /// 获取或设置用于管理此 <see cref="ViewModelBase"/> 实例的 <see cref="IViewModelProvider"/>。
        /// 可能为 null，表示此 <see cref="ViewModelBase"/> 独立存在，未被 View 使用。
        /// </summary>
        IViewModelProvider IViewModelProviderConnectionTarget.Provider
        {
            get => _provider;
            set => _provider = value;
        }

        /// <summary>
        /// 获取一个值。该值指示与此 ViewModel 关联的 View 是否有任何一个当前正在显示并使用此 ViewModel。
        /// </summary>
        protected bool AnyViewLoaded => _loadingCount > 0;

        /// <summary>
        /// 显示与指定的 ViewModel <typeparamref name="TViewModel"/> 关联的 View。
        /// </summary>
        /// <typeparam name="TViewModel">ViewModel 的契约类型。</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        protected void ShowView<TViewModel>()
        {
            VerifyProvider();
            _provider?.ShowViewFrom<TViewModel>(this);
        }

        /// <summary>
        /// 关闭关联此 ViewModel 的 View。这将关闭窗口或者导航返回 Page。
        /// </summary>
        protected void CloseView()
        {
            VerifyProvider();
            _provider?.CloseViewFrom(this);
        }

        void IViewModelProviderConnectionTarget.Attach()
        {
            if (!_isAttached)
            {
                _isAttached = true;
                OnAttached();
            }
        }

        void IViewModelProviderConnectionTarget.Load()
        {
            _loadingCount++;
            if (_loadingCount == 1)
            {
                OnLoaded();
            }
        }

        void IViewModelProviderConnectionTarget.Unload()
        {
            _loadingCount--;
            if (_loadingCount == 0)
            {
                OnUnloaded();
            }

            if (_loadingCount < 0)
            {
                _loadingCount = 0;
                throw new InvalidOperationException("Unload 的次数大于 Load 的次数，可能出现了 XAML 系统的 BUG。");
            }
        }

        /// <summary>
        /// 在派生类中重写此方法时，指定 View 将此 ViewModel 设置为其 DataContext 时应该进行的初始化操作。
        /// </summary>
        protected virtual void OnAttached()
        {
        }

        /// <summary>
        /// 在派生类中重写此方法时，指定 View 布局完成并显示时 ViewModel 应该进行的初始化操作。
        /// </summary>
        protected virtual void OnLoaded()
        {
        }

        /// <summary>
        /// 在派生类中重写此方法时，指定 View 从视觉树中移除或窗口关闭时 ViewModel 应该进行的反初始化操作。
        /// </summary>
        protected virtual void OnUnloaded()
        {
        }

        /// <summary>
        /// 从指定的 Model 中获取更新的值到此 ViewModel 上。
        /// 如果 ViewModel 中需要更新的属性名称与 Model 中不一致，则需要使用 <see cref="ModelPropertyAttribute"/> 标记 Model 中的属性名。
        /// </summary>
        /// <typeparam name="T">Model 的类型。</typeparam>
        /// <param name="valueSource">Model 的实例。</param>
        protected void UpdateProperties<T>(T valueSource)
        {
            var sourceType = typeof(T);
            var viewModelType = GetType();

            var sourceProperties = sourceType.GetRuntimeProperties().ToList();
            var viewModelProperties = viewModelType.GetRuntimeProperties();

            foreach (var property in viewModelProperties.Where(x => x.CanWrite))
            {
                var modelName = property.GetCustomAttribute<ModelPropertyAttribute>()
                                    ?.ModelPropertyName ?? property.Name;
                var sourceProperty = sourceProperties.Find(x => x.Name == modelName);
                if (sourceProperty != null)
                {
                    property.SetValue(this, sourceProperty.GetValue(valueSource));
                }
            }
        }

        [Conditional("DEBUG")]
        private void VerifyProvider()
        {
            if (_provider == null)
            {
                throw new InvalidOperationException(
                    "此 ViewModel 未被统一管理。只有在同一个 ViewModelProvider 管理下的 ViewModel 之间才可以发生关联。");
            }
        }

        /// <summary>
        /// 记录此 ViewModel 的实例被多少个 View 以显式方式使用。
        /// 仅指定但未显示的不计在内。
        /// </summary>
        private int _loadingCount;

        /// <summary>
        /// 记录此 ViewModel 的实例是否已被设置为 View 的 DataContext。
        /// </summary>
        private bool _isAttached;

        /// <summary>
        /// 记录关联到此 <see cref="ViewModelBase"/> 的 <see cref="IViewModelProvider"/>。
        /// 如果没有任何 <see cref="IViewModelProvider"/> 关联到此实例，此值将保持为 null。
        /// </summary>
        [CanBeNull] private IViewModelProvider _provider;
    }
}
