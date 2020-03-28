using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
    /// <inheritdoc />
    /// <summary>
    /// 为 <see cref="ViewModelProvider" /> 提供简易的高性能的依赖反转方案。
    /// </summary>
    internal sealed class CompositionHost : IContainer
    {
        /// <summary>
        /// 指示容器中是否共享同一个实例。
        /// </summary>
        private readonly bool _sharingInstances;

        /// <summary>
        /// 记录可以进行导出的类型及其可以延迟创建的实例。
        /// </summary>
        [NotNull]
        private readonly Dictionary<Type, Lazy<object>> _exports = new Dictionary<Type, Lazy<object>>();

        /// <summary>
        /// 记录可以进行导出的类型及其创建实例的方法。
        /// </summary>
        [NotNull]
        private readonly Dictionary<Type, Func<object>> _creates = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// 无论类型是否能导出，都记录其收集到的类型信息。将来导出和注入都需要使用。
        /// </summary>
        [NotNull]
        private readonly Dictionary<Type, IContractInfo> _exportInfoDictionary = new Dictionary<Type, IContractInfo>();

        /// <summary>
        /// 使用收集到的 <see cref="IContractInfo"/> 集合创建 <see cref="CompositionHost"/> 的新实例。
        /// </summary>
        /// <param name="diContracts">收集到的所有依赖信息集合。</param>
        /// <param name="sharingInstances">指示容器中是否共享同一个实例。</param>
        internal CompositionHost([ItemNotNull] [NotNull] IEnumerable<IContractInfo> diContracts,
            bool sharingInstances = false)
        {
            _sharingInstances = sharingInstances;
            Append(diContracts);
        }

        /// <summary>
        /// 额外添加收集到的 <see cref="IContractInfo"/> 集合。
        /// </summary>
        /// <param name="diContracts">收集到的所有依赖信息集合。</param>
        internal void Append([NotNull] IEnumerable<IContractInfo> diContracts)
        {
            if (diContracts == null) throw new ArgumentNullException(nameof(diContracts));

            foreach (var export in diContracts)
            {
                var contractType = export.ContractType;
                if (contractType != null)
                {
                    _exports[contractType] = new Lazy<object>(() => export.Create(),
                        LazyThreadSafetyMode.ExecutionAndPublication);
                    _creates[contractType] = () => export.Create();
                }

                _exportInfoDictionary[export.RealType] = export;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// 为依赖收集者提供依赖注入的方法。
        /// </summary>
        /// <typeparam name="TExport">需要注入到目标对象的类型。</typeparam>
        /// <returns>可以注入到目标属性或参数的实例。</returns>
        [NotNull]
        TExport IContainer.Import<TExport>() => GetExport<TExport>();

        /// <summary>
        /// 获取导出的 <typeparamref name="TExport"/> 类型的实例。
        /// </summary>
        /// <typeparam name="TExport">导出的契约类型，真实类型是其子类或接口的实现。</typeparam>
        /// <returns><typeparamref name="TExport"/> 类型的实例。</returns>
        [NotNull]
        internal TExport GetExport<TExport>()
        {
            return (TExport)GetExport(typeof(TExport));
        }

        /// <summary>
        /// 获取契约类型为 <paramref name="contractType"/> 类型的实例，契约类型和实际类型之间不一定存在继承关系。
        /// </summary>
        /// <param name="contractType">契约类型。</param>
        /// <returns>标记了指定契约的类型的实例。</returns>
        [NotNull]
        internal object GetExport([NotNull] Type contractType)
        {
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));

            //判断是否共享实例
            var isTypeOfShareInstance = typeof(IShareInstance).IsAssignableFrom(contractType);
            var isSharingInstance = _sharingInstances || isTypeOfShareInstance;

            //如果需要共享实例 且 导出类型中存在指定类型，则直接从字典中获取实例。否则，创建新实例
            if (isSharingInstance && _exports.TryGetValue(contractType, out var lazy))
            {
                return lazy.Value;
            }

            if (_creates.TryGetValue(contractType, out var create))
            {
                return create();
            }

            throw new InvalidOperationException($"获取类型（{contractType.FullName}）的实例之前，必须先导出类型元数据。");
        }

        /// <summary>
        /// 将 <paramref name="exportSource"/> 中导出的实例注入到 <paramref name="instance"/> 实例中。
        /// 如果 <paramref name="exportSource"/> 实例为 null，则将此 <see cref="CompositionHost"/> 中导出的实例进行注入。
        /// </summary>
        /// <param name="instance">需要进行依赖注入的对象。</param>
        /// <param name="exportSource">导出契约类型的容器。</param>
        internal void SatisfyImports([NotNull] object instance, [CanBeNull] CompositionHost exportSource = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            exportSource = exportSource ?? this;
            foreach (var type in FindPublicTypes(instance))
            {
                if (_exportInfoDictionary.TryGetValue(type, out var info))
                {
                    info.Satisfy(instance, exportSource);
                }
            }
        }

        /// <summary>
        /// 寻找 <paramref name="object"/> 的实际类型所有可能成为契约类型的类型对象。
        /// </summary>
        /// <param name="object">用于寻找契约类型的实例。</param>
        /// <returns>可能的契约类型的枚举器。</returns>
        [ItemNotNull]
        private static IEnumerable<Type> FindPublicTypes([NotNull] object @object)
        {
            // 首先返回真实类型，因为 CompositionHost 默认使用真实类型来注入依赖。
            // 第一个返回的类型为真实类型能够大概率命中，显著降低枚举次数。
            var type = @object.GetType();
            yield return type;

            // 其次返回接口类型，因为 View-ViewModel 以接口类型作为契约更符合面向对象的设计。
            // 让良好设计的代码获得更好的性能是应该的。
            foreach (var @interface in type.GetInterfaces())
            {
                yield return @interface;
            }

            // 最后依次返回基类，因为没有指定任何契约的类型可能使用通用基类进行依赖注入的契约匹配。
            var @base = type.BaseType;
            while (@base != null && @base != typeof(object))
            {
                yield return @base;
                @base = @base.BaseType;
            }
        }
    }
}
