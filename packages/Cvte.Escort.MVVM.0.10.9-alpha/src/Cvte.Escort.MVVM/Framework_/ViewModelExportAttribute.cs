using System;

namespace Cvte.Escort
{
    /// <summary>
    /// 在 ViewModel 上指定此特性以便声明这是一个可与 View 关联的 ViewModel。
    /// 被 View 隐式关联后的 ViewModel 可以有限地控制 View；否则将无法在直接控制。
    /// </summary>
    public sealed class ViewModelExportAttribute : Attribute
    {
        /// <summary>
        /// 在 ViewModel 上指定此特性以便声明这是一个可与 View 关联的 ViewModel。
        /// 被 View 隐式关联后的 ViewModel 可以有限地控制 View；否则将无法在直接控制。
        /// </summary>
        public ViewModelExportAttribute() : base()
        {
        }
    }
}
