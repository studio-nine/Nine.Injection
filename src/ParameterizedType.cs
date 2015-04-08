namespace Nine.Injection
{
    using System;

    struct ParameterizedType
    {
        public Type Type;
        public object[] Parameters;

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

            var count = Parameters.Length;
            for (var i = 0; i < count; i++)
            {
                if (!Equals(Parameters[i], other.Parameters[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            ulong hash = 2166136261U;

            hash ^= (ulong)Type.GetHashCode();

            if (Parameters != null)
            {
                var count = Parameters.Length;
                for (var i = 0; i < count; i++)
                {
                    var item = Parameters[i];
                    if (item == null) continue;
                    hash ^= (ulong)item.GetHashCode();
                    hash *= 16777619U;
                }
            }

            return (int)hash;
        }
    }
}
