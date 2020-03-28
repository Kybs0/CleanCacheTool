namespace Cvte.Escort
{
    /// <summary>
    /// 如果类型会被 <see cref="IViewModelProvider"/> 管理，实现此方法将获得 <see cref="IViewModelProvider"/> 的实例。
    /// </summary>
    internal interface IViewModelProviderConnectionTarget
    {
        /// <summary>
        /// 此属性会被 <see cref="IViewModelProvider"/> 赋值，以便在接口的实现类中获得管理此类型的 <see cref="IViewModelProvider"/> 的实例。
        /// 通过 <see cref="IViewModelProvider"/> 的实例可以与 View 和 ViewModel 发生关联。
        /// </summary>
        IViewModelProvider Provider { get; set; }

        /// <summary>
        /// 此方法会被 <see cref="IViewModelProvider"/> 调用，表示至少有一个 View 开始关联此 ViewModel。
        /// 在 <see cref="IViewModelProvider"/> 的实现中，此方法需保证只调用一次，且在 <see cref="Load"/> 之前调用。
        /// </summary>
        void Attach();

        /// <summary>
        /// 此方法会被 <see cref="IViewModelProvider"/> 调用，表示任何一个 View 刚开始完成显示；只要有新的 View 显示，都需要调用此方法。
        /// </summary>
        void Load();

        /// <summary>
        /// 此方法会被 <see cref="IViewModelProvider"/> 调用，表示任何一个 View 不再显示；只要有 View 关闭，都需要调用此方法。
        /// </summary>
        void Unload();
    }
}
