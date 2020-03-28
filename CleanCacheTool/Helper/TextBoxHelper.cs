using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CleanCacheTool.Helper
{
    public static class TextBoxHelper
    {
        #region 设置文本超出显示时，文本显示是否靠右显示

        /// <summary>
        /// 当文本超出显示时，文本是否靠右侧显示
        /// </summary>
        public static readonly DependencyProperty ScrollEndWhenTextTrimmedProperty = DependencyProperty.RegisterAttached(
        "ScrollEndWhenTextTrimmed", typeof(bool), typeof(TextBoxHelper),
        new PropertyMetadata(default(bool), OnScrollEndWhenTextTrimmedChanged));

        private static void OnScrollEndWhenTextTrimmedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TextBox)d;
            textBox.TextChanged -= TextBoxOnTextChanged;
            if ((bool)e.NewValue)
            {
                SetFlowDirection();
                textBox.TextChanged += TextBoxOnTextChanged;
            }
            void TextBoxOnTextChanged(object sender, TextChangedEventArgs args)
            {
                SetFlowDirection();
            }

            void SetFlowDirection()
            {
                textBox.FlowDirection = IsTextTrimmed(textBox) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }
        }

        /// <summary>
        /// 判断文本框文本是否超出显示
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        private static bool IsTextTrimmed(TextBox textBox)
        {
            Typeface typeface = new Typeface(
                textBox.FontFamily,
                textBox.FontStyle,
                textBox.FontWeight,
                textBox.FontStretch);

            FormattedText formattedText = new FormattedText(
                textBox.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBox.FlowDirection,
                typeface,
                textBox.FontSize,
                textBox.Foreground);

            //判断文本是否超出显示区域（单行）
            bool isTrimmed = formattedText.Width > textBox.Width;

            return isTrimmed;
        }

        public static void SetScrollEndWhenTextTrimmed(DependencyObject element, bool value)
        {
            element.SetValue(ScrollEndWhenTextTrimmedProperty, value);
        }

        public static bool GetScrollEndWhenTextTrimmed(DependencyObject element)
        {
            return (bool)element.GetValue(ScrollEndWhenTextTrimmedProperty);
        }

        #endregion
    }
}
