using System;

namespace Cvte.Escort
{
    /// <summary>
    /// 在 View 的类型上指定此特性可以让 ViewModel 发现此 View，以便在 View 还未创建时创建并控制此 View。
    /// </summary>
    public sealed class ViewModelConnectAttribute : Attribute
    {
        /// <summary>
        /// 获取连接到 ViewModel 的契约类型。
        /// </summary>
        public Type ContractType { get; }

        /// <summary>
        /// 在 View 的类型上指定此特性可以让 ViewModel 发现此 View，以便在 View 还未创建时创建并控制此 View。
        /// </summary>
        /// <param name="contractType">ViewModel 的契约类型。即 XAML 中用于查找 ViewModel 实例时采用的类型。</param>
        public ViewModelConnectAttribute(Type contractType)
        {
            ContractType = contractType;
        }
    }
}
