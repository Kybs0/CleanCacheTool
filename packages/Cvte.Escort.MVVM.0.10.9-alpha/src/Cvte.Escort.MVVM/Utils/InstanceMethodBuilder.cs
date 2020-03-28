using System;
using System.Linq;
using System.Reflection;
using Cvte.Escort.Annotations;

namespace Cvte.Escort
{
    /// <summary>
    /// 为类型的实例方法创建调用委托，如果方法可能被多次调用，则相比于反射调用的性能更高。
    /// 泛型参数的最后一个是方法的返回值类型，其他都是参数类型。 
    /// </summary>
    internal static class InstanceMethodBuilder<T, TReturnValue>
    {
        #region 委托中不包含实例，调用时需传入具体的实例对象（即补充调用中的 this 参数）

        /// <summary>
        /// 创建用于调用 <typeparamref name="TInstanceType"/> 类型中名为 <paramref name="methodName"/> 方法的委托。
        /// 调用时形如：var result = func(this, t)。
        /// </summary>
        /// <typeparam name="TInstanceType">实例的类型。</typeparam>
        /// <param name="methodName">要调用的方法名。</param>
        /// <returns>用于调用指定实例中指定方法的委托。</returns>
        [System.Diagnostics.Contracts.Pure, MustUseReturnValue, NotNull]
        internal static Func<TInstanceType, T, TReturnValue> CreateMethod<TInstanceType>(string methodName)
        {
            return CreateMethod<TInstanceType>(typeof(TInstanceType).GetRuntimeMethod(methodName,
                new[] { typeof(T) }));
        }

        /// <summary>
        /// 创建用于调用 <typeparamref name="TInstanceType"/> 类型中符合 <paramref name="methodPredicate"/> 要求的方法的委托。
        /// 调用时形如：var result = func(this, t)。
        /// </summary>
        /// <typeparam name="TInstanceType">实例的类型。</typeparam>
        /// <param name="methodPredicate">要调用的方法需要满足的条件。</param>
        /// <returns>用于调用指定实例中指定方法的委托。</returns>
        [System.Diagnostics.Contracts.Pure, MustUseReturnValue, NotNull]
        internal static Func<TInstanceType, T, TReturnValue> CreateMethod<TInstanceType>(
            Func<MethodInfo, bool> methodPredicate)
        {
            return CreateMethod<TInstanceType>(typeof(TInstanceType).GetRuntimeMethods().First(methodPredicate));
        }

        /// <summary>
        /// 创建用于调用 <typeparamref name="TInstanceType"/> 类型中 <paramref name="method"/> 实例方法的委托。
        /// 调用时形如：var result = func(this, t)。
        /// </summary>
        /// <typeparam name="TInstanceType">实例的类型。</typeparam>
        /// <param name="method">要调用的方法。</param>
        /// <returns>用于调用指定实例中指定方法的委托。</returns>
        [System.Diagnostics.Contracts.Pure, MustUseReturnValue, NotNull]
        internal static Func<TInstanceType, T, TReturnValue> CreateMethod<TInstanceType>([NotNull] MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return (Func<TInstanceType, T, TReturnValue>) method.CreateDelegate(
                typeof(Func<TInstanceType, T, TReturnValue>));
        }

        #endregion

        #region 委托中已包含实例本身，调用时无需传入具体的实例对象

        /// <summary>
        /// 创建用于调用 <paramref name="instance"/> 实例中名为 <paramref name="methodName"/> 方法的委托。
        /// 调用方法的实例已在委托生成期间确定。调用时形如：var result = func(t)。
        /// </summary>
        /// <typeparam name="TInstanceType">实例的类型。</typeparam>
        /// <param name="instance">要调用的实例。</param>
        /// <param name="methodName">要调用的方法名。</param>
        /// <returns>用于调用指定实例中指定方法的委托。</returns>
        [System.Diagnostics.Contracts.Pure, MustUseReturnValue, NotNull]
        internal static Func<T, TReturnValue> CreateInstanceMethod<TInstanceType>(
            [NotNull] TInstanceType instance, string methodName)
        {
            return CreateInstanceMethod(instance, typeof(TInstanceType).GetRuntimeMethod(methodName,
                new[] { typeof(T) }));
        }

        /// <summary>
        /// 创建用于调用 <paramref name="instance"/> 实例中符合 <paramref name="methodPredicate"/> 要求的方法的委托。
        /// 调用方法的实例已在委托生成期间确定。调用时形如：var result = func(t)。
        /// </summary>
        /// <typeparam name="TInstanceType">实例的类型。</typeparam>
        /// <param name="instance">要调用的实例。</param>
        /// <param name="methodPredicate">要调用的方法需要满足的条件。</param>
        /// <returns>用于调用指定实例中指定方法的委托。</returns>
        [System.Diagnostics.Contracts.Pure, MustUseReturnValue, NotNull]
        internal static Func<T, TReturnValue> CreateInstanceMethod<TInstanceType>(
            [NotNull] TInstanceType instance, Func<MethodInfo, bool> methodPredicate)
        {
            return CreateInstanceMethod(instance, typeof(TInstanceType).GetRuntimeMethods().First(methodPredicate));
        }

        /// <summary>
        /// 创建用于调用 <paramref name="instance"/> 实例中 <paramref name="method"/> 方法的委托。
        /// 调用方法的实例已在委托生成期间确定。调用时形如：var result = func(t)。
        /// </summary>
        /// <typeparam name="TInstanceType">实例的类型。</typeparam>
        /// <param name="instance">要调用的实例。</param>
        /// <param name="method">要调用的方法。</param>
        /// <returns>用于调用指定实例中指定方法的委托。</returns>
        [System.Diagnostics.Contracts.Pure, MustUseReturnValue, NotNull]
        internal static Func<T, TReturnValue> CreateInstanceMethod<TInstanceType>(
            [NotNull] TInstanceType instance, [NotNull] MethodInfo method)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            return (Func<T, TReturnValue>) method.CreateDelegate(
                typeof(Func<T, TReturnValue>), instance);
        }

        #endregion
    }
}
