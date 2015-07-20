namespace Nine.Injection
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Represents a single record of type mapping.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeMap
    {
        private bool hasValue;
        private WeakReference<object> weakValue;
        private object value;

        internal object[] DefaultParameterOverrides;

        internal void SetValue(object value, bool weak)
        {
            this.hasValue = true;

            if (weak && value != null)
            {
                this.weakValue = new WeakReference<object>(value);
            }
            else
            {
                this.value = value;
            }
        }

        internal bool TryGetValue(out object target)
        {
            if (!hasValue)
            {
                target = null;
                return false;
            }

            if (weakValue != null)
            {
                return weakValue.TryGetTarget(out target);
            }

            target = value;
            return true;
        }

        /// <summary>
        /// Gets the type of interface or class to be registered
        /// </summary>
        public Type From { get; internal set; }

        /// <summary>
        /// Gets the type of concrete class to be instantiated when from is resolved from the container.
        /// </summary>
        public Type To { get; internal set; }

        /// <summary>
        /// Gets whether the type or instance is explicity mapped.
        /// </summary>
        public bool IsExplicit { get; internal set; }

        /// <inheritdoc />
        public override string ToString() => $"{From} -> {To}";
    }
}
