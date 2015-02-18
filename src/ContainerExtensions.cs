namespace Nine.Injection
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContainerExtensions
    {
        /// <summary>
        /// Map a type within the container.
        /// </summary>
        /// <typeparam name="T">The type of class being registered</typeparam>
        /// <returns>The container, complete with new registration</returns>
        public static IContainer Map<T>(this IContainer container)
        {
            container.Map(typeof(T), typeof(T));
            return container;
        }

        /// <summary>
        /// Map an implementation type against an interface or class
        /// </summary>
        /// <typeparam name="TFrom">The type of interface or class to be registered</typeparam>
        /// <typeparam name="TTo">The type of concrete class to be instantiated when <see cref="TFrom" /> is resolved from the container.</typeparam>
        /// <returns>The container, complete with new registration</returns>
        public static IContainer Map<TFrom, TTo>(this IContainer container) where TTo : TFrom
        {
            container.Map(typeof(TFrom), typeof(TTo));
            return container;
        }

        /// <summary>
        /// Map a specific instance of a concrete implementation for an interface or class
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be registered</typeparam>
        /// <param name="instance">The instance to register in the container</param>
        /// <returns>The container, complete with new registration</returns>
        public static IContainer Add<T>(this IContainer container, T instance)
        {
            container.Add(typeof(T), instance);
            return container;
        }

        /// <summary>
        /// Try to resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> if registered, or null</returns>
        public static T Get<T>(this IContainer container) where T : class
        {
            return container.Get(typeof(T)) as T;
        }

        /// <summary>
        /// Gets all registered instances of a specified type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <returns>A collection of registered instances. If no instances are registered, returns empty collection, not null</returns>
        public static IEnumerable<T> GetAll<T>(this IContainer container)
        {
            return (IEnumerable<T>)container.GetAll(typeof(T));
        }

        /// <summary>
        /// Make the container synchronized.
        /// </summary>
        public static IContainer Synchronized(this IContainer container)
        {
            return new SynchronizedContainer(container);
        }
    }
}