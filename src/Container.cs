namespace Nine.Ioc
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class Container : IEnumerable<TypeMap>, IContainer
    {
        private readonly Dictionary<Type, List<TypeMap>> map = new Dictionary<Type, List<TypeMap>>();

        /// <inheritdoc />
        public void Map(Type from, Type to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            GetMappings(from).Add(new TypeMap { From = from, To = to });
        }

        /// <inheritdoc />
        public void Add(Type type, object instance)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var mapping = new TypeMap { From = type, HasValue = true, Value = instance };
            if (instance != null)
            {
                mapping.To = instance.GetType();
            }

            GetMappings(type).Add(mapping);
        }

        /// <inheritdoc />
        public object Get(Type type)
        {
            var mappings = GetMappings(type);

            if (mappings.Count > 0)
            {
                return Get(mappings[mappings.Count - 1]);
            }

            var instance = Instantiate(type);
            var mapping = new TypeMap { From = type, Value = instance, HasValue = true };
            if (instance != null)
            {
                mapping.To = instance.GetType();
            }

            mappings.Add(mapping);

            return instance;
        }

        /// <inheritdoc />
        public IEnumerable<T> GetAll<T>() where T : class
        {
            var type = typeof(T);
            var mapping = GetMappings(type);

            //return mapping.Select(map => map.Value as T ?? Instantiate(map.To));
            throw new NotImplementedException();
        }

        private List<TypeMap> GetMappings(Type type)
        {
            List<TypeMap> result;
            if (!map.TryGetValue(type, out result))
            {
                map.Add(type, result = new List<TypeMap>());
            }
            return result;
        }

        private object Get(TypeMap map)
        {
            if (map.HasValue)
            {
                return map.Value;
            }

            map.HasValue = true;
            return map.Value = Instantiate(map.To);
        }

        private object Instantiate(Type type)
        {
            ConstructorInfo constructor = null;
            ParameterInfo[] parameters = null;

            // Find matching constructor with the biggest number of parameters
            var paramCount = -1;
            foreach (var candidate in type.GetTypeInfo().DeclaredConstructors)
            {
                parameters = candidate.GetParameters();
                if (parameters.Length > paramCount)
                {
                    paramCount = parameters.Length;
                    constructor = candidate;
                }
            }

            if (constructor == null)
            {
                return null;
            }

            if (parameters.Length <= 0)
            {
                return constructor.Invoke(null);
            }

            // Instantiate constructor parameters
            var constructorParams = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                constructorParams[i] = Get(parameters[i].ParameterType);
            }

            return constructor.Invoke(constructorParams);
        }
        
        public IEnumerator<TypeMap> GetEnumerator()
        {
            return map.Values.SelectMany(m => m).Where(m => m.To != null).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}