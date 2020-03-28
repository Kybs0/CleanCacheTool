using System;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
#pragma warning disable CA1000 // Do not declare static members on generic types
    /// <summary>
    /// 为 <see cref="ViewModelConnectAttribute"/> 标记的类型提供连接信息的收集。
    /// </summary>
    /// <typeparam name="TConnect">连接的契约类型。</typeparam>
    public static class ViewDI<TConnect>
    {
        /// <summary>
        /// 收集 View 的类型信息，便于 ViewModel 操作此 View。
        /// </summary>
        /// <typeparam name="TReal">View 的实际类型。</typeparam>
        /// <param name="satisfy">将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。</param>
        /// <returns>收集到的一条可供 <see cref="ViewModelProvider"/> 使用的 View/ViewModel 依赖信息。</returns>
        [NotNull, PublicAPI]
        public static IContractInfo Create<TReal>([CanBeNull] Action<TReal, IContainer> satisfy = null)
            where TReal : new()
        {
            return new ConnectDIInfo<TConnect, TReal>(() => new TReal(), satisfy);
        }
    }

    /// <summary>
    /// 为普通的 View 提供收集依赖注入信息的方法。
    /// </summary>
    public static class ViewDI
    {
        /// <summary>
        /// 收集 View 的类型信息，便于对此 View 中的部分属性进行依赖注入。
        /// </summary>
        /// <typeparam name="TReal">View 的实际类型。</typeparam>
        /// <param name="satisfy">将依赖注入到 <typeparamref name="TReal"/> 对象的实例的方法。</param>
        /// <returns>收集到的一条可供 <see cref="ViewModelProvider"/> 使用的 View/ViewModel 依赖信息。</returns>
        [NotNull, PublicAPI]
        public static IContractInfo Create<TReal>([CanBeNull] Action<TReal, IContainer> satisfy = null)
        {
            return new ImportOnlyDIInfo<TReal>(satisfy);
        }
    }
#pragma warning restore CA1000 // Do not declare static members on generic types
}
