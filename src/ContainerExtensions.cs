namespace Nine.Injection
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// Contains extension method for <see cref="IContainer"/> interface.
    /// </summary>
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
        /// <typeparam name="TTo">The type of concrete class to be instantiated when <typeparamref name="TFrom"/> is resolved from the container.</typeparam>
        /// <returns>The container, complete with new registration</returns>
        public static IContainer Map<TFrom, TTo>(this IContainer container) where TTo : TFrom
        {
            container.Map(typeof(TFrom), typeof(TTo));
            return container;
        }

        /// <summary>
        ///  Map all exported types in the assemblies that implements T or derives from T.
        /// </summary>
        /// <returns>The container, complete with new registration</returns>
        public static IContainer MapAll<T>(this IContainer container, params Assembly[] assemblies)
        {
            IEnumerable<Type> exportedTypes = null;

            foreach (var assembly in assemblies)
            {
                try
                {
                    exportedTypes = assembly.ExportedTypes;
                }
                catch (Exception)
                {
                    continue;
                }

                MapAll<T>(container, exportedTypes);
            }

            return container;
        }

        /// <summary>
        ///  Map all types that implements T or derives from T.
        /// </summary>
        /// <returns>The container, complete with new registration</returns>
        public static IContainer MapAll<T>(this IContainer container, IEnumerable<Type> types)
        {
            var ti = typeof(T).GetTypeInfo();

            foreach (var type in types)
            {
                var info = type.GetTypeInfo();
                if (IsTypeInjectible(info) && ti.IsAssignableFrom(info))
                {
                    container.Map(typeof(T), type);
                }
            }

            return container;
        }

        private static bool IsTypeInjectible(TypeInfo type)
        {
            return type.IsClass && type.IsVisible && !type.IsAbstract && !type.IsGenericType && !type.IsGenericTypeDefinition;
        }

        /// <summary>
        /// Map a specific instance of a concrete implementation for an interface or class
        /// </summary>
        /// <param name="container">The container</param>
        /// <typeparam name="T">The type of interface or class to be registered</typeparam>
        /// <param name="instance">The instance to register in the container</param>
        /// <returns>The container, complete with new registration</returns>
        public static IContainer Map<T>(this IContainer container, T instance)
        {
            container.Map(typeof(T), instance);
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