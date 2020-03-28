using System;

namespace Cvte.Escort
{
    /// <summary>
    /// 在允许访问 ViewModel 的类型中，在字段、属性、方法参数上指定此特性可以获得依赖的一个 ViewModel 的实例。
    /// </summary>
    public sealed class ViewModelImportAttribute : Attribute
    {
        /// <summary>
        /// 在允许访问 ViewModel 的类型中，在字段、属性、方法参数上指定此特性可以获得依赖的一个 ViewModel 的实例。
        /// </summary>
        public ViewModelImportAttribute()
        {
        }
    }
}
