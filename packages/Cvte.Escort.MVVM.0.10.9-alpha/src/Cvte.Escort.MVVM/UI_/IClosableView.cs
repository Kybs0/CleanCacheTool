namespace Cvte.Escort
{
    /// <summary>
    /// 当在 View 中实现时，如果有 ViewModel 申请关闭 View，则会调用此接口的实现。
    /// </summary>
    public interface IClosableView
    {
        /// <summary>
        /// 当 ViewModel 发起了关闭 View 的指令后，如果此 View 符合关闭要求，将会找到此 View 并将其关闭。
        /// 关闭可能指的是设置可见性为“隐藏”。
        /// </summary>
        void CloseView();
    }

    //public interface IClosableView<T> where T : Message
    //{
    //    void Close(T message);
    //}
}
