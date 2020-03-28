using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Cvte.Escort.Annotations;
#pragma warning disable 1591

namespace Cvte.Escort
{
    public class ViewFrame : FrameworkElement
    {
        /// <summary>
        /// 获取预加载的元素集合。
        /// </summary>
        public UIElementCollection PreservedChildren { get; }

        public ViewFrame()
        {
            PreservedChildren = new UIElementCollection(this, this);
        }

        // TODO WeakReference
        private readonly Stack<UIElement> _backStack = new Stack<UIElement>();

        [CanBeNull]
        private object _navigatingParameter;

        [CanBeNull, ContractPublicPropertyName(nameof(Child))]
        private UIElement _child;

        /// <summary>
        /// 获取或设置当前正在显示的 View。 
        /// </summary>
        [CanBeNull, DefaultValue((UIElement) null)]
        public UIElement Child
        {
            get => _child;
            set
            {
                // 通知并决定是否取消导航。
                var old = _child;
                // ReSharper disable SuspiciousTypeConversion.Global
                var oldPage = old as IPage;
                var newPage = value as IPage;
                // ReSharper restore SuspiciousTypeConversion.Global
                var cancelArgs = new NavigatingCancelEventArgs(old, value, _navigatingParameter);
                oldPage?.OnNavigatingFrom(cancelArgs);
                if (cancelArgs.IsCanceled)
                {
                    return;
                }

                // 从逻辑树和视觉树移除不在预加载集合中的元素。
                if (old != null && !PreservedChildren.Cast<UIElement>().Contains(old))
                {
                    RemoveVisualChild(old);
                    RemoveLogicalChild(old);
                }

                // 导航。
                _child = value;

                // 如果当前元素不在预加载集合，则将其加入逻辑树和视觉树。
                if (value != null && !PreservedChildren.Cast<UIElement>().Contains(value))
                {
                    AddVisualChild(value);
                    AddLogicalChild(value);
                }

                // 通知导航完成。
                var args = new NavigationEventArgs(old, value, _navigatingParameter);
                oldPage?.OnNavigatedFrom(args);
                newPage?.OnNavigatedTo(args);

                InvalidateArrange();
            }
        }

        /// <summary>
        /// 导航到 View。
        /// </summary>
        public void Navigate([CanBeNull] UIElement target, [CanBeNull] object parameter = null)
        {
            _navigatingParameter = parameter;

            try
            {
                InnerNavigate(target);
            }

            finally
            {
                _navigatingParameter = null;
            }
        }

        /// <summary>
        /// 返回上一个显示的 View。
        /// </summary>
        public void GoBack()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (Child is IViewAggregator viewAggregator && viewAggregator.GoBack())
            {
                // 如果 Child 是一个View聚合器，则执行其 GoBack 方法；
                // 若 viewAggregator.GoBack() 返回为 false ，则执行默认的 GoBack 逻辑
                return;
            }
            else
            {
                if (_backStack.Any())
                {
                    var child = _backStack.Pop();
                    Child = child;
                }
            }
        }

        private void InnerNavigate(UIElement target)
        {
            // 如果当前 Chlid 可以完成对 target 的导航，则首先让其导航，不成功则继续。
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (Child is IViewAggregator viewAggregator && target != null && viewAggregator.Contain(target.GetType()))
            {
                viewAggregator.Navigate(target.GetType(), target);
                return;
            }

            // 如果 PreservedChildren 有元素能够完成对 target 的导航，则让其导航，不成功则继续。
            var preservedElement = PreservedChildren.Cast<UIElement>().ToList();
            foreach (UIElement uiElement in preservedElement)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (uiElement is IViewAggregator vAggregator && target != null && vAggregator.Contain(target.GetType()))
                {
                    vAggregator.Navigate(target.GetType(), target);
                    return;
                }
            }

            // 最后使用默认的导航。
            if (Child != null)
            {
                _backStack.Push(Child);
            }
            Child = target;
        }

        private IEnumerable<UIElement> UISortedChildren
        {
            get
            {
                // 处于最底层的是：被保留，但不在导航栈中的页面。
                foreach (var notNavigatedPreservedChild in PreservedChildren
                    .Cast<UIElement>().Where(x => !_backStack.Contains(x) && !Equals(x, Child)))
                {
                    yield return notNavigatedPreservedChild;
                }

                // 处于中间层的是：被保留，也处于导航栈中的页面（要求按照导航栈的顺序显示）。
                foreach (var navigatedPreservedChild in _backStack
                    .Where(x => PreservedChildren.Contains(x) && !Equals(x, Child)))
                {
                    yield return navigatedPreservedChild;
                }

                // 处于最顶层的是：Child。
                if (Child != null)
                {
                    yield return Child;
                }
            }
        }

        protected override int VisualChildrenCount => UISortedChildren.Count();

        protected override Visual GetVisualChild(int index) => UISortedChildren.ElementAt(index);

        protected override IEnumerator LogicalChildren => UISortedChildren.GetEnumerator();

        protected override Size MeasureOverride(Size constraint)
        {
            foreach (var child in UISortedChildren)
            {
                child.Measure(constraint);
            }
            return Child?.DesiredSize ?? default;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (var child in UISortedChildren)
            {
                child.Arrange(new Rect(arrangeSize));
            }
            return arrangeSize;
        }
    }
}
