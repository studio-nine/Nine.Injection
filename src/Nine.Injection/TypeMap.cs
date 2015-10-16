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
        private bool _hasValue;
        private WeakReference<object> _weakValue;
        private object _value;

        internal object[] DefaultParameterOverrides;

        internal void SetValue(object value, bool weak)
        {
            _hasValue = true;

            if (weak && value != null)
            {
                _weakValue = new WeakReference<object>(value);
            }
            else
            {
                _value = value;
            }
        }

        internal bool TryGetValue(out object target)
        {
            if (!_hasValue)
            {
                target = null;
                return false;
            }

            if (_weakValue != null)
            {
                return _weakValue.TryGetTarget(out target);
            }

            target = _value;
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
