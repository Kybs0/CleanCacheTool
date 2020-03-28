using System.Diagnostics.CodeAnalysis;

namespace Cvte.Escort
{
    /// <summary>
    /// 标记一个类型作为 ViewModel 使用。
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Scope = "type", Target = "Cvte.Escort.IViewModel")]
    public interface IViewModel
    {
    }
}
