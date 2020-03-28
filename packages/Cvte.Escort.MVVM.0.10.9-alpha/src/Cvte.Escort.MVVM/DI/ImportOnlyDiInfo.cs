using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
    /// <inheritdoc />
    /// <summary>
    /// 收集到的仅需要注入实例，而没有任何对外契约的类型。
    /// </summary>
    /// <typeparam name="TReal">实际类型。</typeparam>
    internal sealed class ImportOnlyDIInfo<TReal> : IContractInfo
    {
        /// <summary>
        /// 创建 <see cref="ImportOnlyDIInfo{TReal}"/> 的新实例。
        /// </summary>
        /// <param name="satisfy">将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。</param>
        internal ImportOnlyDIInfo([CanBeNull] Action<TReal, IContainer> satisfy = null)
        {
            _satisfy = satisfy;
        }

        /// <summary>
        /// 将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。
        /// </summary>
        [CanBeNull]
        private readonly Action<TReal, IContainer> _satisfy;

        /// <inheritdoc />
        Type IContractInfo.ContractType => null;

        /// <inheritdoc />
        Type IContractInfo.RealType => typeof(TReal);

        /// <inheritdoc />
        object IContractInfo.Create() => throw new NotSupportedException("仅供导入的类型无法创建新实例。");

        /// <inheritdoc />
        void IContractInfo.Satisfy(object instance, IContainer di) => _satisfy?.Invoke((TReal) instance, di);
    }
}
