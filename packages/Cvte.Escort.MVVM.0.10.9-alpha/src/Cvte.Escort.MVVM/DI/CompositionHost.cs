using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
    /// <inheritdoc />
    /// <summary>
    /// Ϊ <see cref="ViewModelProvider" /> �ṩ���׵ĸ����ܵ�������ת������
    /// </summary>
    internal sealed class CompositionHost : IContainer
    {
        /// <summary>
        /// ָʾ�������Ƿ���ͬһ��ʵ����
        /// </summary>
        private readonly bool _sharingInstances;

        /// <summary>
        /// ��¼���Խ��е��������ͼ�������ӳٴ�����ʵ����
        /// </summary>
        [NotNull]
        private readonly Dictionary<Type, Lazy<object>> _exports = new Dictionary<Type, Lazy<object>>();

        /// <summary>
        /// ��¼���Խ��е��������ͼ��䴴��ʵ���ķ�����
        /// </summary>
        [NotNull]
        private readonly Dictionary<Type, Func<object>> _creates = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// ���������Ƿ��ܵ���������¼���ռ�����������Ϣ������������ע�붼��Ҫʹ�á�
        /// </summary>
        [NotNull]
        private readonly Dictionary<Type, IContractInfo> _exportInfoDictionary = new Dictionary<Type, IContractInfo>();

        /// <summary>
        /// ʹ���ռ����� <see cref="IContractInfo"/> ���ϴ��� <see cref="CompositionHost"/> ����ʵ����
        /// </summary>
        /// <param name="diContracts">�ռ���������������Ϣ���ϡ�</param>
        /// <param name="sharingInstances">ָʾ�������Ƿ���ͬһ��ʵ����</param>
        internal CompositionHost([ItemNotNull] [NotNull] IEnumerable<IContractInfo> diContracts,
            bool sharingInstances = false)
        {
            _sharingInstances = sharingInstances;
            Append(diContracts);
        }

        /// <summary>
        /// ��������ռ����� <see cref="IContractInfo"/> ���ϡ�
        /// </summary>
        /// <param name="diContracts">�ռ���������������Ϣ���ϡ�</param>
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
        /// Ϊ�����ռ����ṩ����ע��ķ�����
        /// </summary>
        /// <typeparam name="TExport">��Ҫע�뵽Ŀ���������͡�</typeparam>
        /// <returns>����ע�뵽Ŀ�����Ի������ʵ����</returns>
        [NotNull]
        TExport IContainer.Import<TExport>() => GetExport<TExport>();

        /// <summary>
        /// ��ȡ������ <typeparamref name="TExport"/> ���͵�ʵ����
        /// </summary>
        /// <typeparam name="TExport">��������Լ���ͣ���ʵ�������������ӿڵ�ʵ�֡�</typeparam>
        /// <returns><typeparamref name="TExport"/> ���͵�ʵ����</returns>
        [NotNull]
        internal TExport GetExport<TExport>()
        {
            return (TExport)GetExport(typeof(TExport));
        }

        /// <summary>
        /// ��ȡ��Լ����Ϊ <paramref name="contractType"/> ���͵�ʵ������Լ���ͺ�ʵ������֮�䲻һ�����ڼ̳й�ϵ��
        /// </summary>
        /// <param name="contractType">��Լ���͡�</param>
        /// <returns>�����ָ����Լ�����͵�ʵ����</returns>
        [NotNull]
        internal object GetExport([NotNull] Type contractType)
        {
            if (contractType == null) throw new ArgumentNullException(nameof(contractType));

            //�ж��Ƿ���ʵ��
            var isTypeOfShareInstance = typeof(IShareInstance).IsAssignableFrom(contractType);
            var isSharingInstance = _sharingInstances || isTypeOfShareInstance;

            //�����Ҫ����ʵ�� �� ���������д���ָ�����ͣ���ֱ�Ӵ��ֵ��л�ȡʵ�������򣬴�����ʵ��
            if (isSharingInstance && _exports.TryGetValue(contractType, out var lazy))
            {
                return lazy.Value;
            }

            if (_creates.TryGetValue(contractType, out var create))
            {
                return create();
            }

            throw new InvalidOperationException($"��ȡ���ͣ�{contractType.FullName}����ʵ��֮ǰ�������ȵ�������Ԫ���ݡ�");
        }

        /// <summary>
        /// �� <paramref name="exportSource"/> �е�����ʵ��ע�뵽 <paramref name="instance"/> ʵ���С�
        /// ��� <paramref name="exportSource"/> ʵ��Ϊ null���򽫴� <see cref="CompositionHost"/> �е�����ʵ������ע�롣
        /// </summary>
        /// <param name="instance">��Ҫ��������ע��Ķ���</param>
        /// <param name="exportSource">������Լ���͵�������</param>
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
        /// Ѱ�� <paramref name="object"/> ��ʵ���������п��ܳ�Ϊ��Լ���͵����Ͷ���
        /// </summary>
        /// <param name="object">����Ѱ����Լ���͵�ʵ����</param>
        /// <returns>���ܵ���Լ���͵�ö������</returns>
        [ItemNotNull]
        private static IEnumerable<Type> FindPublicTypes([NotNull] object @object)
        {
            // ���ȷ�����ʵ���ͣ���Ϊ CompositionHost Ĭ��ʹ����ʵ������ע��������
            // ��һ�����ص�����Ϊ��ʵ�����ܹ���������У���������ö�ٴ�����
            var type = @object.GetType();
            yield return type;

            // ��η��ؽӿ����ͣ���Ϊ View-ViewModel �Խӿ�������Ϊ��Լ����������������ơ�
            // ��������ƵĴ����ø��õ�������Ӧ�õġ�
            foreach (var @interface in type.GetInterfaces())
            {
                yield return @interface;
            }

            // ������η��ػ��࣬��Ϊû��ָ���κ���Լ�����Ϳ���ʹ��ͨ�û����������ע�����Լƥ�䡣
            var @base = type.BaseType;
            while (@base != null && @base != typeof(object))
            {
                yield return @base;
                @base = @base.BaseType;
            }
        }
    }
}
