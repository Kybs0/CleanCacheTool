using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
    /// <summary>
    /// 当初始化 <see cref="ViewModelProvider"/> 时，使用此接口的集合声明可以匹配的 View 和 ViewModel。
    /// </summary>
    public interface IContractInfo
    {
        /// <summary>
        /// 获取契约类型。
        /// 作为导出类，通常这是导出类实现的业务接口；作为连接类，通常这是连接类使用 <see cref="Attribute"/> 标记的接口。
        /// （连接代表，此类型可能与导出类发生某些交互行为。）
        /// </summary>
        [CanBeNull]
        Type ContractType { get; }

        /// <summary>
        /// 获取实际类型。
        /// <see cref="ViewModelProvider"/> 需要知道导出类和连接类的实际类型，以便能够直接构造此类型的新实例。
        /// </summary>
        [NotNull]
        Type RealType { get; }

        /// <summary>
        /// 创建 <see cref="RealType"/> 类型的新实例。
        /// </summary>
        /// <returns><see cref="RealType"/> 类型的新实例。</returns>
        [NotNull]
        object Create();

        /// <summary>
        /// 从 <paramref name="di"/> 中取出导出的类型，并注入到 <paramref name="instance"/> 实例中。
        /// </summary>
        /// <param name="instance"><see cref="RealType"/> 类型的实例。</param>
        /// <param name="di">用于获取导出类型的依赖注入容器。</param>
        void Satisfy([NotNull] object instance, [NotNull] IContainer di);
    }
}
