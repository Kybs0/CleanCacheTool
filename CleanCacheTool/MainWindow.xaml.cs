using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using CleanCacheTool.Api;
using CleanCacheTool.Helper;
using CleanCacheTool.Views;
using CleanCacheTool.Views.Models;
using Application = System.Windows.Forms.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;

namespace CleanCacheTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string IniSection = "ViewSetting";
        public MainWindow()
        {
            InitializeComponent();
            //Loaded += MainWindow_Loaded;
            ViewTabControl.SelectedItem = ViewTabControl.Items.Cast<TabItem>().FirstOrDefault();
            Loaded += (s, e) =>
            {
                var iniFilePath = CustomPath.GetUserSettingIniPath();
                Task.Run(() =>
                {
                    CleanCacheVeiwModel.Instance.RefreshCommand.Execute(null);
                });
                //获取配置清理路径
                var viewName = IniUtility.ReadValue(IniSection, IniSection, iniFilePath);
                if (string.IsNullOrWhiteSpace(viewName))
                {
                    MainViewGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    MainViewGrid.Visibility = viewName == nameof(MainViewGrid) ? Visibility.Visible : Visibility.Collapsed;
                }
            };
        }

        #region NotifyIcon

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            SetNotifyIcon();
        }

        private void SetNotifyIcon()
        {
            var notifyIcon = new NotifyIcon();
            notifyIcon.BalloonTipText = "磁盘清理工具";
            notifyIcon.ShowBalloonTip(2000);
            notifyIcon.Text = "磁盘清理工具:每20天清理一次";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Visible = true;
            //打开菜单项
            MenuItem open = new MenuItem("打开");
            open.Click += new EventHandler(Show);
            //退出菜单项
            MenuItem exit = new MenuItem("退出");
            exit.Click += new EventHandler(Close);
            //关联托盘控件
            MenuItem[] childen = new MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new ContextMenu(childen);

            notifyIcon.MouseDoubleClick += new MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Show(o, e);
            });

            this.Closed += (s, e) =>
            {
                notifyIcon?.Dispose();
            };
        }

        private void Show(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.ShowInTaskbar = true;
            this.Activate();
        }

        private void Hide(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;
        }

        private void Close(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region 窗口

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HeaderGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        #endregion

        private void MoreOperationsButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewGrid.Visibility = Visibility.Collapsed;
            var iniFilePath = CustomPath.GetUserSettingIniPath();
            IniUtility.ClearSection(IniSection, iniFilePath);
        }

        private void CleanButton_OnClick(object sender, RoutedEventArgs e)
        {
            CleanCacheVeiwModel.Instance.CleanCacheByMainViewCommand.Execute(null);
        }

        private void TransferDesktopButton_OnClick(object sender, RoutedEventArgs e)
        {
            var currentDesktopFolder = DiskSlimmingViewModel.Instance.GetCurrentDesktopFolder();
            var defaultDektopFolder = DiskSlimmingViewModel.Instance.GetDefaultDektopFolder();
            if (currentDesktopFolder != defaultDektopFolder)
            {
                MessageBox.Show(this, "已经转移过桌面啦！");
                return;
            }

            if (Directory.Exists(@"D:\Desktop"))
            {
                DiskSlimmingViewModel.Instance.DestFolder = @"D:\Desktop";
            }
            else if (Directory.Exists(@"E:\Desktop"))
            {
                DiskSlimmingViewModel.Instance.DestFolder = @"E:\Desktop";
            }
            else if (Directory.Exists(@"F:\Desktop"))
            {
                DiskSlimmingViewModel.Instance.DestFolder = @"F:\Desktop";
            }
            else if (Directory.Exists(@"G:\Desktop"))
            {
                DiskSlimmingViewModel.Instance.DestFolder = @"G:\Desktop";
            }
            else if (Directory.Exists(@"H:\Desktop"))
            {
                DiskSlimmingViewModel.Instance.DestFolder = @"H:\Desktop";
            }
            else if (Directory.Exists(@"J:\Desktop"))
            {
                DiskSlimmingViewModel.Instance.DestFolder = @"J:\Desktop";
            }
            else if (Directory.Exists(@"K:\Desktop"))
            {
                DiskSlimmingViewModel.Instance.DestFolder = @"K:\Desktop";
            }
            else
            {
                MessageBox.Show(this, "检测到貌似只有C盘！如果有其它硬盘，请进入高级选项，手动转移！");
                return;
            }

            DiskSlimmingViewModel.Instance.TransferDesktopCommand.Execute(null);
        }

        private void BackToMainViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewGrid.Visibility = Visibility.Visible;
            var iniFilePath = CustomPath.GetUserSettingIniPath();
            IniUtility.WriteValue(IniSection, IniSection, nameof(MainViewGrid), iniFilePath);
        }
    }
}
