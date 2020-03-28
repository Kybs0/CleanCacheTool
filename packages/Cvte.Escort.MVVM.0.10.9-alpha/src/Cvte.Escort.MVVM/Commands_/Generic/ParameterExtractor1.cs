using System;
using System.Threading.Tasks;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.Commands
{
    /// <summary>
    /// 为 <see cref="Escort"/> 中的命令参数提供泛型解析器。
    /// </summary>
    internal static partial class ParameterExtractor
    {
#if GENERATED_CODE
        /// <summary>
        /// 从命令的 <see cref="object"/> 参数中解析出一个或多个泛型参数值。由于解析过程可能是异步的，所以此方法也必须是异步的。
        /// 参数 <paramref name="parameter"/> 不允许为 null。
        /// </summary>
#else
        /// <summary>
        /// 从命令的 <see cref="object"/> 参数中解析出一个或多个泛型参数值。由于解析过程可能是异步的，所以此方法也必须是异步的。
        /// 原则上，参数 <paramref name="parameter"/> 不允许为 null，但由于其可能为值类型，所以没有抛出异常进行限定。
        /// </summary>
#endif
        [NotNull]
        internal static async Task<(T, bool canExecute)> ExtractAsync<T>(
#if GENERATED_CODE
            [NotNull]
#endif
            object parameter)
        {
            switch (parameter)
            {
                case ValueTuple<T> valueTuple:
#if GENERATED_CODE
                    var t = valueTuple;
                    return (t, true);
#else
                    return (valueTuple.Item1, true);
#endif
                case IAsyncParameterProvider<T> provider:
                    return await FetchParameterAsync(provider).ConfigureAwait(false);
                case IParameterProvider<T> provider:
                    return FetchParameter(provider);
#if GENERATED_CODE
                // 生成的代码由于 object 参数中确定有多个参数值需要提取，于是此 object 一定不可能为 null，否则根本不可能提取出多个值。
                case null:
                    throw new ArgumentNullException(nameof(parameter));
                default:
                    throw new NotSupportedException("必须使用 IParameterProvider 接口提供参数才能执行。");
#else
                // 单个泛型的参数，由于 object 本身可能就是参数所需的值，而此时并不能判定具体业务中 null 是否是传递中的有效值，所以不进行 null 判定。
                default:
                    return ((T) parameter, true);
#endif
            }
        }

        /// <summary>
        /// 从 <see cref="IParameterProvider{T}"/> 提取单个或多个参数。
        /// </summary>
        private static (T, bool canExecute) FetchParameter<T>(IParameterProvider<T> provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            var context = new CommandContext();
            var t = provider.ProvideParameter(context);
            if (context.IsCanceled)
                return (t, false);

            return (t, true);
        }

        /// <summary>
        /// 从 <see cref="IAsyncParameterProvider{T}"/> 异步提取单个或多个参数。
        /// </summary>
        private static async Task<(T, bool canExecute)> FetchParameterAsync<T>(IAsyncParameterProvider<T> provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            var context = new CommandContext();
            var t = await provider.ProvideParameterAsync(context).ConfigureAwait(false);
            if (context.IsCanceled)
                return (t, false);

            return (t, true);
        }
    }
}
