using System.Threading.Tasks;
using Cvte.Escort.Annotations;

// ReSharper disable TypeParameterCanBeVariant

namespace Cvte.Escort.Commands
{
    /// <summary>
    /// 为 <see cref="Escort"/> 中的命令提供参数。
    /// </summary>
    public interface IParameterProvider<T>
    {
        /// <summary>
        /// 当命令需要参数时，将执行此方法，并提供对命令信息控制的上下文信息。
        /// </summary>
        /// <param name="context">可以对命令的执行过程进行控制的上下文信息。</param>
        T ProvideParameter([NotNull] CommandContext context);
    }

    /// <summary>
    /// 为 <see cref="Escort"/> 中的命令提供可以异步获取的参数。
    /// </summary>
    public interface IAsyncParameterProvider<T>
    {
        /// <summary>
        /// 当命令需要参数时，将异步执行此方法，并提供对命令信息控制的上下文信息。
        /// </summary>
        /// <param name="context">可以对命令的执行过程进行控制的上下文信息。</param>
        Task<T> ProvideParameterAsync([NotNull] CommandContext context);
    }
}
