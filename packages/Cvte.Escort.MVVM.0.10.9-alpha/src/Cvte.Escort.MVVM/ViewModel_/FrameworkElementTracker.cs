#if NET45
using System.Windows;

namespace Cvte.Escort
{
    /// <summary>
    /// 用于追踪<see cref="FrameworkElement"/>是否存在于视觉树中。
    /// </summary>
    internal class FrameworkElementTracker
    {
        /// <summary>
        /// 用于指示<see cref="FrameworkElement"/>是否存在于视觉树中。
        /// </summary>
        public static readonly DependencyProperty IsOnVisualTreeProperty = DependencyProperty.RegisterAttached(
            "IsOnVisualTree", typeof(bool), typeof(FrameworkElementTracker), new PropertyMetadata(default(bool)));

        /// <summary>
        /// 创建追踪<paramref name="element"/>的Tracker。
        /// </summary>
        /// <param name="element">被追踪的<see cref="FrameworkElement"/></param>
        public void Track(FrameworkElement element)
        {
            if (!element.IsLoaded)
            {
                element.Loaded += Element_Loaded;
            }
        }

        /// <summary>
        /// 元素加载之后，为其添加附件属性<see cref="IsOnVisualTreeProperty"/>，指示其在视觉树中。
        /// </summary>
        private void Element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            element.Loaded -= Element_Loaded;
            element.SetValue(IsOnVisualTreeProperty, true);
            element.Unloaded += Element_Unloaded;
        }

        /// <summary>
        /// 元素加载之后，将其<see cref="IsOnVisualTreeProperty"/>属性清除。
        /// </summary>
        private void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            element.Unloaded -= Element_Unloaded;
            element.ClearValue(IsOnVisualTreeProperty);
        }

        /// <summary>
        /// 获取一个值，该值表示<paramref name="element"/>是否存在于视觉树中。
        /// （如果该元素没有被追踪过，则返回 false。）
        /// </summary>
        /// <returns>True:元素在视觉树中; False:元素没有在视觉树中或该元素没有被追踪过。</returns>
        public bool IsOnVisualTree(FrameworkElement element)
        {
            return (bool) element.GetValue(IsOnVisualTreeProperty);
        }
    }
}
#endif