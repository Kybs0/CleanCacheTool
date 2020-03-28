using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace Cvte.Escort.Utils
{
    /// <summary>
    /// 为 <see cref="MarkupExtension"/> 中以来对象和依赖属性的赋值过程提供通用的验证措施。
    /// </summary>
    internal static class MarkupExtensionValidator
    {
        /// <summary>
        /// 验证 <see cref="MarkupExtension"/> 中此时提供的服务是否正在向 <see cref="FrameworkElement"/> 的依赖属性赋值。
        /// </summary>
        /// <param name="extension">需要进行通用验证的 <see cref="MarkupExtension"/>。</param>
        /// <param name="serviceProvider">在 <see cref="MarkupExtension.ProvideValue"/> 方法中传入的参数。</param>
        /// <param name="element">如果正在向 <see cref="FrameworkElement"/> 提供值，则返回此 <see cref="FrameworkElement"/> 实例。</param>
        /// <returns>如果验证为此时可以赋值，则返回 true；如果因为设计时支持或者服务对象不存在，则返回 false。</returns>
        internal static bool ValidateMarkupExtension(this MarkupExtension extension,
            IServiceProvider serviceProvider, out FrameworkElement element)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var extensionName = extension.GetType().Name;

            // 设计时直接创建新实例。
            if (DesignerProperties.IsInDesignModeProperty.DefaultMetadata.DefaultValue is true)
            {
                element = null;
                return false;
            }

            // 如果没有服务，则直接返回。
            if (!(serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service))
            {
                element = null;
                return false;
            }

            // MarkupExtension 在样式模板中，返回 this 以延迟提供值。
            if (service.TargetObject.ToString().EndsWith("SharedDp", StringComparison.Ordinal))
            {
                throw new NotSupportedException($"样式和模板不支持 {extensionName}，请在 XAML 次根级元素中使用。");
            }
            if (service.TargetObject is SetterBase)
            {
                throw new NotSupportedException($"属性 Setter.Value 不支持 {extensionName}，请在 XAML 次根级元素中使用。");
            }
            if (!(service.TargetObject is FrameworkElement frameworkElement))
            {
                throw new NotSupportedException("只有 FrameworkElement 才能支持使用 ViewModel 设置 DataContext。");
            }

            element = frameworkElement;
            return true;
        }
    }
}
