using Cvte.Escort.Commands;

namespace Cvte.Escort
{
    /// <summary>
    /// 在 <see cref="IParameterProvider{T}"/> 提供值的过程中对提供流程进行控制。
    /// </summary>
    public class CommandContext
    {
        /// <summary>
        /// 获取一个值，该值指示命令获取参数的过程中是否发现无法获取参数导致命令被取消。
        /// </summary>
        public bool IsCanceled { get; private set; }

        /// <summary>
        /// 取消提供值，这将同时取消命令的执行（如果命令支持）。
        /// 在 <see cref="Escort"/> 中，所有的命令都支持被此方法取消。
        /// </summary>
        public void Cancel()
        {
            IsCanceled = true;
        }
    }
}
