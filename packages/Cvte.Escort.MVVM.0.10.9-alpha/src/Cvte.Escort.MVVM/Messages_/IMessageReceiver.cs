namespace Cvte.Escort
{
    /// <summary>
    /// 表示 <see cref="IMessage"/> 通信实体的接收者。
    /// </summary>
    /// <typeparam name="TMessage">具体可接收的消息类型。</typeparam>
    public interface IMessageReceiver<in TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// 当接收到消息时将执行此方法以处理收到的信息。
        /// </summary>
        /// <param name="message">收到的消息。</param>
        void OnMessageReceived(TMessage message);
    }
}
