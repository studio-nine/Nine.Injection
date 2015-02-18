namespace Nine.Ioc
{
    using System;
    using System.Collections.Generic;

    public interface IContainer
    {
        /// <summary>
        /// Maps a type within the container.
        /// </summary>
        /// <param name="from">The type of interface or class to be registered</param>
        /// <param name="to">The type of concrete class to be instantiated when from is resolved from the container.</param>
        void Map(Type from, Type to);

        /// <summary>
        /// Maps a specific instance of a concrete implementation for an interface or class
        /// </summary>
        /// <param name="type">The type of interface or class to be registered</param>
        /// <param name="instance">The instance to register in the container</param>
        /// <returns>The container, complete with new registration</returns>
        void Add(Type type, object instance);

        /// <summary>
        /// Try to resolve an instance of the specified interface (or class) Type
        /// </summary>
        /// <param name="type">The type of interface or class to be resolved</param>
        /// <returns>An instance of <paramref name="type"/> if registered, or null</returns>
        object Get(Type type);

        /// <summary>
        /// Gets all registered instances of a specified type
        /// </summary>
        /// <typeparam name="T">The type of interface or class to be resolved</typeparam>
        /// <returns>A collection of registered instances. If no instances are registered, returns empty collection, not null</returns>
        IEnumerable<T> GetAll<T>() where T : class;
    }
}
