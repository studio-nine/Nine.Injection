namespace Nine.Injection
{
    using System;
    using System.Collections.Generic;

    struct ParameterizedType
    {
        public Type Type;
        public object[] Parameters;
        public IEqualityComparer<object> EqualityComparer;
        
        public ParameterizedType(Type type, object[] parameters, IEqualityComparer<object> equalityComparer) : this()
        {
            this.Type = type;
            this.Parameters = parameters;
            this.EqualityComparer = equalityComparer;
        }

        public override string ToString() => $"{ Type.Name }";

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is ParameterizedType))
            {
                return false;
            }

            var other = (ParameterizedType)obj;
            if (Type != other.Type)
            {
                return false;
            }

            if ((Parameters == null || Parameters.Length == 0) &&
                (other.Parameters == null || other.Parameters.Length == 0))
            {
                return true;
            }

            if (Parameters == null || other.Parameters == null)
            {
                return false;
            }

            var count = Parameters.Length;
            for (var i = 0; i < count; i++)
            {
                if (EqualityComparer != null)
                {
                    if (!EqualityComparer.Equals(Parameters[i], other.Parameters[i]))
                    {
                        return false;
                    }
                }
                else if (!Equals(Parameters[i], other.Parameters[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = Type.GetHashCode();

            if (Parameters != null)
            {
                var count = Parameters.Length;
                for (var i = 0; i < count; i++)
                {
                    var item = Parameters[i];
                    if (item == null) continue;
                    hash = (hash << 5) + hash;

                    hash ^= EqualityComparer != null
                          ? EqualityComparer.GetHashCode(item) 
                          : item.GetHashCode();
                }
            }

            return hash;
        }
    }
}
