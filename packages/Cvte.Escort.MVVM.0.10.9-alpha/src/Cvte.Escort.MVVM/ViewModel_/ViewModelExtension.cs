using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using Cvte.Escort.Annotations;
using Cvte.Escort.Utils;

namespace Cvte.Escort
{
    /// <inheritdoc />
    /// <summary>
    /// 为 XAML 中的 View 提供 ViewModel 实体的绑定。
    /// 使用方法为 DataContext="{escort:ViewModel XxxViewModel}"。其中 XxxViewModel 为契约类型（不一定是真实类型）。
    /// </summary>
    public class ViewModelExtension : MarkupExtension
    {
        /// <summary>
        /// 包含 ViewModel 契约类型到 <see cref="ViewModelProvider.Resolve{T}"/> 泛型方法调用的缓存。
        /// Key：<see cref="ViewModelProvider"/> 的实例与 ViewModel 契约类型组成的元组。
        /// Value：用以调用 Key.provider 实例中 <see cref="ViewModelProvider.Resolve{T}"/> 方法的委托。
        /// </summary>
        private static readonly
            Dictionary<(object provider, Type viewModelContractType), Func<FrameworkElement, object>>
            CachedGenericResolverDictionary =
                new Dictionary<(object provider, Type viewModelContractType), Func<FrameworkElement, object>>();

        /// <inheritdoc />
        /// <summary>
        /// 创建 <see cref="Cvte.Escort.ViewModelExtension" /> 的新实例，应该由 XAML 解析器创建。
        /// </summary>
        public ViewModelExtension()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// 创建 <see cref="Cvte.Escort.ViewModelExtension" /> 的新实例，应该由 XAML 解析器创建。
        /// </summary>
        /// <param name="viewModelContractType"></param>
        public ViewModelExtension(Type viewModelContractType)
        {
            ViewModelContractType = viewModelContractType ?? throw new ArgumentNullException(nameof(viewModelContractType));
        }

        /// <summary>
        /// 获取或设置 ViewModel 的契约类型。
        /// </summary>
        [CanBeNull, ConstructorArgument("viewModelContractType")]
        public Type ViewModelContractType { get; set; }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this.ValidateMarkupExtension(serviceProvider, out var element))
            {
                return CreateViewModel(element);
            }

            return CreateFallbackValue();
        }

        private object CreateFallbackValue()
        {
            return ViewModelContractType is null ? null : Activator.CreateInstance(ViewModelContractType);
        }

        private object CreateViewModel(FrameworkElement element)
        {
            if (ViewModelContractType == null)
            {
                return null;
            }

            if (!CachedGenericResolverDictionary.TryGetValue((ViewModelProvider.Current, ViewModelContractType),
                out var resolve))
            {
                // 如果没有可以用来解依赖的方法被缓存，则生成一个用于调用
                // ViewModelProvider.Resolve<TViewModel>(FrameworkElement)
                // 的委托。
                resolve = InstanceMethodBuilder<FrameworkElement, object>.CreateInstanceMethod(
                    ViewModelProvider.Current, typeof(ViewModelProvider)
                        .GetRuntimeMethod(nameof(ViewModelProvider.Resolve), new[] { typeof(FrameworkElement) })
                        .MakeGenericMethod(ViewModelContractType));

                CachedGenericResolverDictionary[(ViewModelProvider.Current, ViewModelContractType)] = resolve;
            }

            return resolve(element);
        }
    }
}
