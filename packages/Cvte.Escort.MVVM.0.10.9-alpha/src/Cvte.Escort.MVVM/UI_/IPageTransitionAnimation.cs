using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#pragma warning disable 1591

namespace Cvte.Escort
{
    /// <summary>
    /// 页面切换动画参数
    /// </summary>
    public interface IPageTransitionAnimation
    {
        /// <summary>
        /// 进入方向
        /// </summary>
        PageTransitionDirection InDirection { get; }

        /// <summary>
        /// 离开方向
        /// </summary>
        PageTransitionDirection OutDirection { get; }

        /// <summary>
        /// 切页动画执行时机
        /// </summary>
        PageTransitionMode InOutMode { get; }
    }

    /// <summary>
    /// 切页动画方向
    /// </summary>
    public enum PageTransitionDirection
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom
    }

    /// <summary>
    /// 切页动画执行时机
    /// </summary>
    public enum PageTransitionMode
    {
        /// <summary>
        /// 仅进入时执行动画
        /// </summary>
        OnlyIn,
        
        /// <summary>
        /// 仅离开时执行动画
        /// </summary>
        OnlyOut,

        /// <summary>
        /// 进入和离开都执行动画
        /// </summary>
        BothInOut
    }
}
