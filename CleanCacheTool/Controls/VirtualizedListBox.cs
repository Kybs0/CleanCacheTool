using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CleanCacheTool.Controls
{
    /// <summary>
    /// 针对虚拟化处理的ListBox
    /// </summary>
    class VirtualizedListBox : AwesomeListBox
    {

        #region 虚化化选中

        private bool _isRefreshingSelectedItems = false;

        #region 重新绑定ItemsSource时，更新SelectedItems列表

        /// <summary>
        /// 当前选中课件列表
        /// 注：解决重新绑定课件列表后，因listBox虚拟化ViewModel中的选中状态未能更新界面SelectedItems的问题。
        /// 解决方式：由绑定列表筛选出选中列表后，更新SelectedItems
        /// </summary>
        public static readonly DependencyProperty SelectedCoursewaresProperty = DependencyProperty.Register(
            "SelectedCoursewares", typeof(IList), typeof(VirtualizedListBox), new PropertyMetadata(
                new ObservableCollection<object>(),
                (d, e) => ((VirtualizedListBox)d).RefreshSelectedItems()));

        private void RefreshSelectedItems()
        {
            DoActionWhenSelecting(() =>
            {
                //ItemsSource重新绑定时，设置SelectedItems
                var newCustomSelectedItems = SelectedCoursewares;
                if (SelectionMode == SelectionMode.Multiple)
                {
                    SelectedItems.Clear();
                    foreach (var newSelectedItem in newCustomSelectedItems)
                    {
                        SelectedItems.Add(newSelectedItem);
                    }
                }
                else
                {
                    if (newCustomSelectedItems.Count == 0)
                    {
                        SelectedItem = null;
                    }
                    else
                    {
                        SelectedItem = newCustomSelectedItems[0];
                    }
                }
            });
        }

        private void DoActionWhenSelecting(Action action)
        {
            if (_isRefreshingSelectedItems)
            {
                return;
            }

            try
            {
                _isRefreshingSelectedItems = true;

                action.Invoke();
            }
            catch (Exception e)
            {
                // ignored
            }
            finally
            {
                _isRefreshingSelectedItems = false;
            }
        }
        /// <summary>
        /// 自定义的列表选中项
        /// TODO 可以更新为新名称CustomSelectedItems
        /// </summary>
        public IList SelectedCoursewares
        {
            get
            {
                return (IList)GetValue(SelectedCoursewaresProperty);
            }
            set => SetValue(SelectedCoursewaresProperty, value);
        }

        #endregion

        #region 列表选中时，更新列表IsSelected状态

        /// <summary>
        /// 解决云课件列表列表选中混乱问题
        /// why:界面更新SelectedItems时，因虚拟化导致不在当前视觉内的Item并没有将选中状态删除
        /// how:重写SelectionChanged，对比前后SelectedItems，将之前一部分SelectedItems的选中状态删除。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            DoActionWhenSelecting(() =>
            {
                base.OnSelectionChanged(e);

                bool isVirtualizing = VirtualizingPanel.GetIsVirtualizing(this);

                if (isVirtualizing && (e.AddedItems.Count > 0 || e.RemovedItems.Count > 0))
                {
                    var customSelectItems = SelectedCoursewares;

                    var addedItems = e.AddedItems;
                    foreach (var addedItem in addedItems)
                    {
                        customSelectItems.Add(addedItem);
                    }

                    var deletedItems = e.RemovedItems;
                    foreach (var deletedItem in deletedItems)
                    {
                        customSelectItems.Remove(deletedItem);
                    }

                    SelectedCoursewares = customSelectItems;
                }
            });
        }

        #endregion

        #endregion

    }
}
