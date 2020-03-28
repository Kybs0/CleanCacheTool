using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 在 ViewModel 的属性上指定此特性，使得 ViewModel 可以从 Model 中自动获取更新的值。
    /// 当调用 <see cref="ViewModelBase.UpdateProperties{T}"/> 时，会使用到此特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ModelPropertyAttribute : Attribute
    {
        /// <summary>
        /// 获取用于更新对应 ViewModel 指定属性的 Model 的属性名称。
        /// </summary>
        public string ModelPropertyName { get; }

        /// <summary>
        /// 在 ViewModel 的属性上指定此特性，使得 ViewModel 可以从 Model 中自动获取更新的值。
        /// </summary>
        /// <param name="modelPropertyName">
        /// 指定 Model 中属性的名称，指定后，ViewModel 会从 Model 的此名称的属性中获取更新的值。
        /// 建议使用 nameof 关键字。
        /// </param>
        public ModelPropertyAttribute([NotNull] string modelPropertyName)
        {
            if (modelPropertyName == null)
                throw new ArgumentNullException(nameof(modelPropertyName));
            if (string.IsNullOrWhiteSpace(modelPropertyName))
                throw new ArgumentException("不应该使用空字符串指定属性名称。", nameof(modelPropertyName));

            ModelPropertyName = modelPropertyName;
        }
    }
}
