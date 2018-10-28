using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace CleanCacheTool
{
    /// <summary>
    /// CleanCacheView.xaml 的交互逻辑
    /// </summary>
    public partial class CleanCacheView : UserControl
    {
        public CleanCacheView()
        {
            InitializeComponent();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorTextBox.ScrollToEnd();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            //System.Diagnostics.Process.Start("explorer.exe", currentDirectory);
            System.Diagnostics.Process.Start("Explorer.exe", "/select," + currentDirectory + "\\" + "User.ini");
        }
    }
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool IsReverse { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = value != null && (bool)value ? Visibility.Visible : Visibility.Collapsed;
            return IsReverse
                ? (visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible) : visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
