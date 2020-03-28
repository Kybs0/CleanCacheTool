namespace Cvte.Escort
{
    /// <summary>
    /// 表示一个 UI 控件的行为像页面一样可被导航。
    /// </summary>
    public interface IPage
    {
        /// <summary>
        /// Invoked immediately after the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <param name="e">
        /// Event data that can be examined by overriding code. The event data is representative of the navigation that has unloaded the current Page.
        /// </param>
        void OnNavigatedFrom(NavigationEventArgs e);

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        /// <param name="e">
        /// Event data that can be examined by overriding code. The event data is representative of the pending navigation that will load the current Page. Usually the most relevant property to examine is Parameter.
        /// </param>
        void OnNavigatedTo(NavigationEventArgs e);

        /// <summary>
        /// Invoked immediately before the Page is unloaded and is no longer the current source of a parent Frame.
        /// </summary>
        /// <param name="e">
        /// Event data that can be examined by overriding code. The event data is representative of the navigation that will unload the current Page unless canceled. The navigation can potentially be canceled by setting Cancel.
        /// </param>
        void OnNavigatingFrom(NavigatingCancelEventArgs e);
    }
}
