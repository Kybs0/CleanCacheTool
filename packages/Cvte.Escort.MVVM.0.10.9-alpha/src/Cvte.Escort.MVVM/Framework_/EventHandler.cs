using System;
using System.Threading.Tasks;

namespace Cvte
{
    /// <summary>
    /// 表示指定类型的对象引发了不带数据的事件。
    /// </summary>
    public delegate void TypedEventHandler<in TType>(TType sender, EventArgs e);

    /// <summary>
    /// 表示指定类型的对象引发了带 <typeparamref name="TEventArgs"/> 类型数据的事件。
    /// </summary>
    public delegate void TypedEventHandler<in TType, in TEventArgs>(TType sender, TEventArgs e)
        where TEventArgs : EventArgs;

    /// <summary>
    /// 表示引发异步执行的不带数据的事件。通常不应该存在这种类型的事件。
    /// </summary>
    public delegate Task AsyncEventHandler(object sender, EventArgs e);

    /// <summary>
    /// 表示引发异步执行的带 <typeparamref name="TEventArgs"/> 类型数据的事件。这通常意味着参数中存在可能被异步修改的属性。
    /// </summary>
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e)
        where TEventArgs : EventArgs;

    /// <summary>
    /// 表示指定类型的对象引发异步执行的不带数据的事件。通常不应该存在这种类型的事件。
    /// </summary>
    public delegate Task AsyncTypedEventHandler<in TType>(TType sender, EventArgs e);

    /// <summary>
    /// 表示指定类型的对象引发异步执行的带 <typeparamref name="TEventArgs"/> 类型数据的事件。这通常意味着参数中存在可能被异步修改的属性。
    /// </summary>
    public delegate Task AsyncTypedEventHandler<in TType, in TEventArgs>(TType sender, TEventArgs e)
        where TEventArgs : EventArgs;
}
