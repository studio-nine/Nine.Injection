namespace Nine.Ioc
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Represents a single record of type mapping.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeMap
    {
        internal bool HasValue;
        internal object Value;

        /// <summary>
        /// Gets the type of interface or class to be registered
        /// </summary>
        public Type From { get; internal set; }

        /// <summary>
        /// Gets the type of concrete class to be instantiated when from is resolved from the container.
        /// </summary>
        public Type To { get; internal set; }
    }
}
