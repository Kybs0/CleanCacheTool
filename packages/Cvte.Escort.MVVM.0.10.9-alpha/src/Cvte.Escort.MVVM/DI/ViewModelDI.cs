using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
#pragma warning disable CA1000 // Do not declare static members on generic types
    /// <summary>
    /// 为 <see cref="ViewModelExportAttribute"/> 标记的类型提供 ViewModel 导出信息的收集。
    /// </summary>
    /// <typeparam name="TExport">导出的契约类型，通常是 ViewModel 实现的业务接口。</typeparam>
    public static class ViewModelDI<TExport>
    {
        /// <summary>
        /// 收集 ViewModel 的类型信息，便于 ViewModel 与 View 之间进行交互。
        /// </summary>
        /// <typeparam name="TReal">ViewModel 的实际类型。</typeparam>
        /// <param name="satisfy">将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。</param>
        /// <returns>收集到的一条可供 <see cref="ViewModelProvider"/> 使用的 View/ViewModel 依赖信息。</returns>
        [NotNull, PublicAPI]
        public static IContractInfo Create<TReal>([CanBeNull] Action<TReal, IContainer> satisfy = null)
            where TReal : TExport, new()
        {
            return new ExportInterfaceDIInfo<TExport, TReal>(satisfy == null
                ? (Action<TExport, IContainer>) null
                : (instance, di) => satisfy((TReal) instance, di));
        }
    }
#pragma warning restore CA1000 // Do not declare static members on generic types
}
