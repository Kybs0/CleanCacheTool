using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CleanCacheTool.Controls
{
    class AwesomeListBox : ListBox
    {
        public AwesomeListBox()
        {
        }


        #region 课件列表是否处于编辑状态

        /// <summary>
        /// 课件列表的编辑状态
        /// </summary>
        public static readonly DependencyProperty DocListStateProperty = DependencyProperty.Register(
            "DocListState", typeof(EditStatus), typeof(AwesomeListBox), new PropertyMetadata(EditStatus.Normal, OnEditStatusChanged));

        private static void OnEditStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox && e.NewValue is EditStatus editStatus)
            {
                if (editStatus == EditStatus.Editing)
                {
                    listBox.SelectionMode = SelectionMode.Multiple;
                }
                else if (editStatus == EditStatus.Normal)
                {
                    //退出编辑状态，取消全选
                    listBox.SelectionMode = SelectionMode.Extended;
                    listBox.UnselectAll();
                }
            }
        }

        public EditStatus DocListState
        {
            get => (EditStatus)GetValue(DocListStateProperty);
            set => SetValue(DocListStateProperty, value);
        }

        #endregion

    }
    /// <summary>
    /// 编辑状态
    /// </summary>
    public enum EditStatus
    {
        /// <summary>
        /// 正常状态
        /// </summary>
        Normal,

        /// <summary>
        /// 正在编辑状态
        /// </summary>
        Editing
    }
}
