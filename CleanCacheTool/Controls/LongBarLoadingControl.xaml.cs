using System.Windows;
using System.Windows.Controls;

namespace CleanCacheTool
{
    /// <summary>
    /// LongBarLoadingControl.xaml 的交互逻辑
    /// </summary>
    public partial class LongBarLoadingControl : UserControl
    {
        public LongBarLoadingControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载提示
        /// </summary>
        public static readonly DependencyProperty LoadingTipsProperty = DependencyProperty.Register(
            "LoadingTips", typeof(string), typeof(LongBarLoadingControl), new PropertyMetadata(default(string)));

        public string LoadingTips
        {
            get => (string)GetValue(LoadingTipsProperty);
            set => SetValue(LoadingTipsProperty, value);
        }

        /// <summary>
        /// 加载提示详细内容
        /// </summary>
        public static readonly DependencyProperty LoadingTipsDetailProperty = DependencyProperty.Register(
            "LoadingTipsDetail", typeof(string), typeof(LongBarLoadingControl), new PropertyMetadata(default(string)));

        public string LoadingTipsDetail
        {
            get => (string)GetValue(LoadingTipsDetailProperty);
            set => SetValue(LoadingTipsDetailProperty, value);
        }

        /// <summary>
        /// 进度(百分比)
        /// </summary>
        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(
            "ProgressValue", typeof(int), typeof(LongBarLoadingControl), new PropertyMetadata(default(int)));

        public int ProgressValue
        {
            get => (int)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        /// <summary>
        /// 进度详细
        /// </summary>
        public static readonly DependencyProperty ProgressValueDetailProperty = DependencyProperty.Register(
            "ProgressValueDetail", typeof(string), typeof(LongBarLoadingControl), new PropertyMetadata(default(string)));

        public string ProgressValueDetail
        {
            get => (string)GetValue(ProgressValueDetailProperty);
            set => SetValue(ProgressValueDetailProperty, value);
        }

        /// <summary>
        /// 完成提示
        /// </summary>
        public static readonly DependencyProperty LoadedTipsProperty = DependencyProperty.Register(
            "LoadedTips", typeof(string), typeof(LongBarLoadingControl), new PropertyMetadata(default(string)));

        public string LoadedTips
        {
            get => (string)GetValue(LoadedTipsProperty);
            set => SetValue(LoadedTipsProperty, value);
        }

        public static readonly DependencyProperty LoadingSuccessedProperty = DependencyProperty.Register(
            "LoadingSuccessed", typeof(bool), typeof(LongBarLoadingControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// 是否加载完成
        /// </summary>
        public bool LoadingSuccessed
        {
            get => (bool)GetValue(LoadingSuccessedProperty);
            set => SetValue(LoadingSuccessedProperty, value);
        }

        public static readonly DependencyProperty IsLoadingTipsDetailSmallProperty = DependencyProperty.Register(
            "IsLoadingTipsDetailSmall", typeof(bool), typeof(LongBarLoadingControl), new PropertyMetadata(default(bool)));

        /// <summary>
        /// 加载详细内容 是否 小字号
        /// </summary>
        public bool IsLoadingTipsDetailSmall
        {
            get => (bool)GetValue(IsLoadingTipsDetailSmallProperty);
            set => SetValue(IsLoadingTipsDetailSmallProperty, value);
        }
    }
}
