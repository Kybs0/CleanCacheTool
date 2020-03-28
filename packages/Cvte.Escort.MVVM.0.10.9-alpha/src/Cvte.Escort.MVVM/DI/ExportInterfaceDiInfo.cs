using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
    /// <inheritdoc />
    /// <summary>
    /// 从实现某契约接口的类型中收集到的类型。
    /// </summary>
    /// <typeparam name="TExport">用于导出的契约类型。</typeparam>
    /// <typeparam name="TReal">实际类型。</typeparam>
    internal sealed class ExportInterfaceDIInfo<TExport, TReal> : IContractInfo where TReal : new()
    {
        /// <summary>
        /// 创建 <see cref="ExportInterfaceDIInfo{TExport,TReal}"/> 类型的新实例。
        /// </summary>
        /// <param name="satisfy">将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。</param>
        internal ExportInterfaceDIInfo([CanBeNull] Action<TExport, IContainer> satisfy = null)
        {
            _satisfy = satisfy;
        }

        /// <summary>
        /// 将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。
        /// </summary>
        [CanBeNull]
        private readonly Action<TExport, IContainer> _satisfy;

        /// <inheritdoc />
        Type IContractInfo.ContractType => typeof(TExport);

        /// <inheritdoc />
        Type IContractInfo.RealType => typeof(TReal);

        /// <inheritdoc />
        object IContractInfo.Create() => new TReal();

        /// <inheritdoc />
        void IContractInfo.Satisfy(object instance, IContainer di) => _satisfy?.Invoke((TExport) instance, di);
    }
}
