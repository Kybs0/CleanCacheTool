using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
    /// <inheritdoc />
    /// <summary>
    /// 从 <see cref="ViewModelConnectAttribute" /> 收集到的类型，包含连接的契约类型。
    /// </summary>
    /// <typeparam name="TConnect">连接的契约类型。</typeparam>
    /// <typeparam name="TReal">实际类型。</typeparam>
    internal sealed class ConnectDIInfo<TConnect, TReal> : IContractInfo
    {
        /// <summary>
        /// 创建 <see cref="ConnectDIInfo{TConnect,TReal}"/> 类型的新实例。
        /// </summary>
        /// <param name="create">创建 <typeparamref name="TReal"/> 类型新实例的方法。</param>
        /// <param name="satisfy">将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。</param>
        internal ConnectDIInfo([NotNull] Func<TReal> create, [CanBeNull] Action<TReal, IContainer> satisfy = null)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _satisfy = satisfy;
        }

        /// <summary>
        /// 创建 <typeparamref name="TReal"/> 类型新实例的方法。
        /// </summary>
        [NotNull]
        private readonly Func<TReal> _create;

        /// <summary>
        /// 将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。
        /// </summary>
        [CanBeNull]
        private readonly Action<TReal, IContainer> _satisfy;

        /// <inheritdoc />
        Type IContractInfo.ContractType => typeof(TConnect);

        /// <inheritdoc />
        Type IContractInfo.RealType => typeof(TReal);

        /// <inheritdoc />
        object IContractInfo.Create() => _create();

        /// <inheritdoc />
        void IContractInfo.Satisfy(object instance, IContainer di) => _satisfy?.Invoke((TReal) instance, di);
    }
}
