using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Cvte.Escort
{
    /// <summary>
    /// 一个View聚合器，该聚合器可以同时将多个View添加到视觉树，以完成提前加载的目的
    /// </summary>
    public interface IViewAggregator
    {
        /// <summary>
        /// 导航到 <paramref name="viewType"/> 对应的View，隐藏其它View
        /// </summary>
        void Navigate(Type viewType, UIElement view);

        /// <summary>
        /// 返回上一个View
        /// </summary>
        /// <returns>成功执行返回True,如果上一个View不是该聚合器中的一个，则返回false</returns>
        bool GoBack();

        /// <summary>
        /// 获取聚合器是否包含 <paramref name="viewType"/> 对应的View
        /// </summary>
        /// <returns></returns>
        bool Contain(Type viewType);
    }

}
