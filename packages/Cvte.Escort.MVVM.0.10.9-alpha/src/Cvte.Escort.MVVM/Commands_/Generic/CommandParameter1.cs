using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cvte.Escort.Annotations;

namespace Cvte.Escort.Commands
{
    /// <summary>
    /// 为 <see cref="Escort"/> 中多泛型的命令提供只读的泛型参数。
    /// </summary>
    public class CommandParameter<T> : IParameterProvider<T>
    {
        /// <summary>
        /// 将传入的参数作为只读的命令参数，以便让 <see cref="ActionCommand{T}"/> 能够使用。
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t")]
        public CommandParameter(T t)
        {
            Value = t;
        }

        /// <summary>
        /// 获取预指定的只读命令参数的值。
        /// 由于部分业务中 null 可能是合理的值，所以在 C#8.0 以下的版本中，值是否为 null 需要业务定义。
        /// </summary>
        public T Value { get; }

        T IParameterProvider<T>.ProvideParameter(CommandContext context)
        {
            return Value;
        }
    }

    /// <summary>
    /// 为 <see cref="Escort"/> 中多泛型的命令提供可异步获取的泛型参数。
    /// </summary>
    public class AsyncCommandParameter<T> : IAsyncParameterProvider<T>
    {
        private readonly Func<Task<T>> _provider;

        /// <summary>
        /// 将传入的参数作为只读的命令参数，以便让 <see cref="ActionCommand{T}"/> 能够使用。
        /// </summary>
        public AsyncCommandParameter(Func<Task<T>> provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// 异步提供多泛型参数值，得到的值将提供给命令作为参数。
        /// </summary>
        [NotNull]
        public async Task<T> ProvideParameterAsync(CommandContext context)
        {
            return await _provider.Invoke().ConfigureAwait(false);
        }
    }
}
