using Cvte.Escort.Annotations;

namespace Cvte.Escort.DI
{
    /// <summary>
    /// 表示 <see cref="Escort"/> 内置的简易依赖注入容器，
    /// 在使用 <see cref="ViewModelProvider"/> 时可在参数中获得此容器的实例。
    /// </summary>
    [PublicAPI]
    public interface IContainer
    {
        /// <summary>
        /// 从 <see cref="ViewModelProvider"/> 中获取一个此上下文中允许被注入的对象。
        /// </summary>
        /// <typeparam name="TContract">需要注入的实例的契约类型。（导出和注入之间使用契约类型进行匹配。）</typeparam>
        /// <returns>注入的契约类型的实例。</returns>
        [PublicAPI]
        TContract Import<TContract>();
    }
}
