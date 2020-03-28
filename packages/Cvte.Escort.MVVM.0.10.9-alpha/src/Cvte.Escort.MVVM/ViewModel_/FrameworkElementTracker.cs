#if NET45
using System.Windows;

namespace Cvte.Escort
{
    /// <summary>
    /// ����׷��<see cref="FrameworkElement"/>�Ƿ�������Ӿ����С�
    /// </summary>
    internal class FrameworkElementTracker
    {
        /// <summary>
        /// ����ָʾ<see cref="FrameworkElement"/>�Ƿ�������Ӿ����С�
        /// </summary>
        public static readonly DependencyProperty IsOnVisualTreeProperty = DependencyProperty.RegisterAttached(
            "IsOnVisualTree", typeof(bool), typeof(FrameworkElementTracker), new PropertyMetadata(default(bool)));

        /// <summary>
        /// ����׷��<paramref name="element"/>��Tracker��
        /// </summary>
        /// <param name="element">��׷�ٵ�<see cref="FrameworkElement"/></param>
        public void Track(FrameworkElement element)
        {
            if (!element.IsLoaded)
            {
                element.Loaded += Element_Loaded;
            }
        }

        /// <summary>
        /// Ԫ�ؼ���֮��Ϊ����Ӹ�������<see cref="IsOnVisualTreeProperty"/>��ָʾ�����Ӿ����С�
        /// </summary>
        private void Element_Loaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            element.Loaded -= Element_Loaded;
            element.SetValue(IsOnVisualTreeProperty, true);
            element.Unloaded += Element_Unloaded;
        }

        /// <summary>
        /// Ԫ�ؼ���֮�󣬽���<see cref="IsOnVisualTreeProperty"/>���������
        /// </summary>
        private void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            element.Unloaded -= Element_Unloaded;
            element.ClearValue(IsOnVisualTreeProperty);
        }

        /// <summary>
        /// ��ȡһ��ֵ����ֵ��ʾ<paramref name="element"/>�Ƿ�������Ӿ����С�
        /// �������Ԫ��û�б�׷�ٹ����򷵻� false����
        /// </summary>
        /// <returns>True:Ԫ�����Ӿ�����; False:Ԫ��û�����Ӿ����л��Ԫ��û�б�׷�ٹ���</returns>
        public bool IsOnVisualTree(FrameworkElement element)
        {
            return (bool) element.GetValue(IsOnVisualTreeProperty);
        }
    }
}
#endif