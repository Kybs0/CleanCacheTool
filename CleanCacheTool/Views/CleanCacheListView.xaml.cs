using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CleanCacheTool.Controls;
using CleanCacheTool.Helper;
using CleanCacheTool.Views.Models;

namespace CleanCacheTool.Views
{
    /// <summary>
    /// CleanCacheView.xaml 的交互逻辑
    /// </summary>
    public partial class CleanCacheListView : UserControl
    {
        public CleanCacheListView()
        {
            InitializeComponent();
//#if DEBUG
//            LogCheckBox.Visibility = Visibility.Visible;
//#endif
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", CustomPath.GetUserSettingIniPath());
        }
        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorTextBox.ScrollToEnd();
        }

        #region 选中

        public static readonly DependencyProperty AllSelectedProperty = DependencyProperty.Register(
            "AllSelected", typeof(bool), typeof(CleanCacheListView), new PropertyMetadata(default(bool)));

        public bool AllSelected
        {
            get { return (bool)GetValue(AllSelectedProperty); }
            set { SetValue(AllSelectedProperty, value); }
        }

        public static readonly DependencyProperty HasItemsSelectedProperty = DependencyProperty.Register(
            "HasItemsSelected", typeof(bool), typeof(CleanCacheListView), new PropertyMetadata(default(bool)));

        public bool HasItemsSelected
        {
            get { return (bool)GetValue(HasItemsSelectedProperty); }
            set { SetValue(HasItemsSelectedProperty, value); }
        }
        private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetItemsStatus(CleanUpListBox.SelectedItems.Count, CleanUpListBox.Items.Count);
        }

        private void SetItemsStatus(int selectedItemsCount, int itemsCount)
        {
            AllSelected = itemsCount > 0 && selectedItemsCount == itemsCount;
            HasItemsSelected = selectedItemsCount > 0;
        }

        #endregion

        #region 全选

        private void AllSelectCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            var isSelected = AllSelectCheckBox.IsChecked ?? false;
            if (isSelected)
            {
                CleanUpListBox.SelectAll();
            }
            else
            {
                CleanUpListBox.UnselectAll();
            }
        }

        #endregion

        #region 多选

        private int _previousSelectIndexForEditing = -1;

        private void OnListBoxItemKeyDown(object sender, KeyEventArgs e)
        {
            var selectedListBoxItem = (ListBoxItem)sender;
            //编辑模式下，Shift键，连续多选
            if (selectedListBoxItem.DataContext != null &&
                _previousSelectIndexForEditing == -1 &&
                (e.Key == Key.LeftShift || e.Key == Key.RightShift))
            {
                int currentSelectIndex = CleanUpListBox.Items.IndexOf(selectedListBoxItem.DataContext);
                _previousSelectIndexForEditing = currentSelectIndex;
            }
        }

        private void OnListBoxItemKeyUp(object sender, KeyEventArgs e)
        {
            //编辑模式下，Shift键，连续多选
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                _previousSelectIndexForEditing = -1;
            }
        }

        /// <summary>
        /// Shift多选
        /// </summary>
        /// <param name="selectedListBoxItem"></param>
        private void SettleSelectedItemsForShift(ListBoxItem selectedListBoxItem)
        {
            int currentSelectIndex = CleanUpListBox.ItemContainerGenerator.IndexFromContainer(selectedListBoxItem);
            if (_previousSelectIndexForEditing >= 0 && currentSelectIndex >= 0)
            {
                int startNum = _previousSelectIndexForEditing;
                int endNum = currentSelectIndex;
                //Shift多选，删除其余选中
                CleanUpListBox.UnselectAll();

                //按顺序添加SelectedItems--因shift多选,listBox自带的选中项列表顺序混乱。
                bool isAssending = endNum - startNum > 0;
                if (isAssending)
                {
                    for (int i = startNum; i <= endNum; i++)
                    {
                        //多选模式下，当前MouseDown之后会触发SelctionChanged
                        if (i == currentSelectIndex)
                        {
                            continue;
                        }
                        CleanUpListBox.SelectedItems.Add(CleanUpListBox.Items[i]);
                    }
                }
                else
                {
                    for (int i = startNum; i >= endNum; i--)
                    {
                        //多选模式下，当前MouseDown之后会触发SelctionChanged
                        if (i == currentSelectIndex)
                        {
                            continue;
                        }
                        CleanUpListBox.SelectedItems.Add(CleanUpListBox.Items[i]);
                    }
                }
            }
        }
        private void OnListBoxItemPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedListBoxItem = (ListBoxItem)sender;

            if (CleanUpListBox.DocListState == EditStatus.Editing
                || e.Source is System.Windows.Controls.Primitives.ButtonBase
                || selectedListBoxItem.DataContext == null)
            {
                return;
            }
            if (selectedListBoxItem.IsSelected && Keyboard.Modifiers == ModifierKeys.None)
            {
                //解决全选或者多选情况下继续单击无反应的问题，取消其它多选项的选中状态
                //切换SelectionMode，可以更新ListBoxItem内部选中逻辑，单选后解决Shift多选残留问题
                CleanUpListBox.SelectedItem = null;
                CleanUpListBox.SelectionMode = SelectionMode.Single;
                selectedListBoxItem.IsSelected = true;
                CleanUpListBox.SelectionMode = SelectionMode.Extended;
            }
        }

        private void OnListBoxItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedListBoxItem = (ListBoxItem)sender;
            //编辑模式下，右键一直保持选中
            if (CleanUpListBox.DocListState == EditStatus.Editing)
            {
                selectedListBoxItem.IsSelected = true;

                e.Handled = true;
            }
        }

        private void OnListBoxItemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var selectedListBoxItem = (ListBoxItem)sender;
            //快捷键Shift多选
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                SettleSelectedItemsForShift(selectedListBoxItem);
            }
        }
        #endregion

        #region 链接

        private void ModuleFolderNameLinkedButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CleaningModuleItem item)
            {
                System.Diagnostics.Process.Start(item.ModuleFolder);
            }
        }

        #endregion

    }
}
