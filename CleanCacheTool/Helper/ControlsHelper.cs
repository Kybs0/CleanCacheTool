using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CleanCacheTool
{
    public class ControlsHelper
    {
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.RegisterAttached(
            "Geometry", typeof(Geometry), typeof(ControlsHelper), new PropertyMetadata(default(Geometry)));

        public static void SetGeometry(DependencyObject element, Geometry value)
        {
            element.SetValue(GeometryProperty, value);
        }

        public static Geometry GetGeometry(DependencyObject element)
        {
            return (Geometry)element.GetValue(GeometryProperty);
        }

        #region 用于描述：控件内部背景色

        public static readonly DependencyProperty BackgroundNormalProperty = DependencyProperty.RegisterAttached(
            "BackgroundNormal", typeof(Brush), typeof(ControlsHelper), new PropertyMetadata(Brushes.Transparent));

        public static void SetBackgroundNormal(DependencyObject element, Brush value)
        {
            element.SetValue(BackgroundNormalProperty, value);
        }

        public static Brush GetBackgroundNormal(DependencyObject element)
        {
            return (Brush)element.GetValue(BackgroundNormalProperty);
        }

        public static readonly DependencyProperty BackgroundHoverProperty = DependencyProperty.RegisterAttached(
            "BackgroundHover", typeof(Brush), typeof(ControlsHelper), new PropertyMetadata(Brushes.Transparent));

        public static void SetBackgroundHover(DependencyObject element, Brush value)
        {
            element.SetValue(BackgroundHoverProperty, value);
        }

        public static Brush GetBackgroundHover(DependencyObject element)
        {
            return (Brush)element.GetValue(BackgroundHoverProperty);
        }

        public static readonly DependencyProperty BackgroundPressedProperty = DependencyProperty.RegisterAttached(
            "BackgroundPressed", typeof(Brush), typeof(ControlsHelper), new PropertyMetadata(Brushes.Transparent));

        public static void SetBackgroundPressed(DependencyObject element, Brush value)
        {
            element.SetValue(BackgroundPressedProperty, value);
        }

        public static Brush GetBackgroundPressed(DependencyObject element)
        {
            return (Brush)element.GetValue(BackgroundPressedProperty);
        }


        #endregion

        #region 用于描述：控件内部文本前景色

        public static readonly DependencyProperty ForegroundNormalProperty = DependencyProperty.RegisterAttached(
            "ForegroundNormal", typeof(Brush), typeof(ControlsHelper), new PropertyMetadata(Brushes.Black));

        public static void SetForegroundNormal(DependencyObject element, Brush value)
        {
            element.SetValue(ForegroundNormalProperty, value);
        }

        public static Brush GetForegroundNormal(DependencyObject element)
        {
            return (Brush)element.GetValue(ForegroundNormalProperty);
        }

        public static readonly DependencyProperty ForegroundHoverProperty = DependencyProperty.RegisterAttached(
            "ForegroundHover", typeof(Brush), typeof(ControlsHelper), new PropertyMetadata(Brushes.Black));

        public static void SetForegroundHover(DependencyObject element, Brush value)
        {
            element.SetValue(ForegroundHoverProperty, value);
        }

        public static Brush GetForegroundHover(DependencyObject element)
        {
            return (Brush)element.GetValue(ForegroundHoverProperty);
        }

        public static readonly DependencyProperty ForegroundPressedProperty = DependencyProperty.RegisterAttached(
            "ForegroundPressed", typeof(Brush), typeof(ControlsHelper), new PropertyMetadata(Brushes.Black));

        public static void SetForegroundPressed(DependencyObject element, Brush value)
        {
            element.SetValue(ForegroundPressedProperty, value);
        }

        public static Brush GetForegroundPressed(DependencyObject element)
        {
            return (Brush)element.GetValue(ForegroundPressedProperty);
        }
        #endregion

        #region 为控件附加圆角属性 CornerRadius

        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius)obj.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(ControlsHelper),
                new PropertyMetadata(default(CornerRadius)));

        #endregion

    }
}
