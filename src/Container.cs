namespace Nine.Injection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a constructor dependency injection container.
    /// </summary>
    public class Container : IContainer
    {
        private bool freezed;
        private readonly object syncRoot = new object();
        private readonly FuncFactory funcFactory;
        private readonly Dictionary<ParameterizedType, List<TypeMap>> mappings = new Dictionary<ParameterizedType, List<TypeMap>>();
        private readonly HashSet<Type> dependencyTracker = new HashSet<Type>();

        /// <inheritdoc />
        public Container()
        {
            funcFactory = new FuncFactory(this);
        }

        /// <summary>
        /// Gets the type mappings managed by this container.
        /// </summary>
        public IEnumerable<TypeMap> Mappings
        {
            get { return mappings.SelectMany(m => m.Value).Where(m => m.To != null); }
        }

        /// <summary>
        /// Freezes this container and returns the freezed (this) instance.
        /// </summary>
        /// <returns></returns>
        public IContainer Freeze()
        {
            freezed = true;
            return this;
        }

        /// <inheritdoc />
        public void Map(Type from, Type to) => Map(from, to, null);

        /// <inheritdoc />
        public void Map(Type from, Type to, params object[] parameterOverrides)
        {
            if (freezed)
            {
                throw new InvalidOperationException("Cannot map a type when the container is freezed");
            }

            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            lock (syncRoot)
            {
                GetMappings(new ParameterizedType { Type = from }).Add(new TypeMap
                {
                    From = from, To = to, DefaultParameterOverrides = parameterOverrides
                });

                if (parameterOverrides != null && parameterOverrides.Length > 0)
                {
                    GetMappings(new ParameterizedType { Type = from, Parameters = parameterOverrides?.ToArray() }).Add(new TypeMap
                    {
                        From = from, To = to, DefaultParameterOverrides = parameterOverrides
                    });
                }
            }
        }

        /// <inheritdoc />
        public void Map(Type type, object instance)
        {
            if (freezed)
            {
                throw new InvalidOperationException("Cannot map a type when the container is freezed");
            }

            lock (syncRoot)
            {
                Map(type, instance, false);
            }
        }

        private void Map(Type type, object instance, bool weak)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var mapping = new TypeMap { From = type };
            mapping.SetValue(instance, weak);
            if (instance != null)
            {
                mapping.To = instance.GetType();
            }

            GetMappings(new ParameterizedType { Type = type }).Add(mapping);
        }

        /// <inheritdoc />
        public object Get(Type type)
        {
            lock (syncRoot)
            {
                dependencyTracker.Clear();
                return GetCore(type, null);
            }
        }

        /// <inheritdoc />
        public object Get(Type type, params object[] parameterOverrides)
        {
            lock (syncRoot)
            {
                dependencyTracker.Clear();
                return GetCore(type, parameterOverrides);
            }
        }

        private object GetCore(Type type, object[] parameterOverrides)
        {
            var ti = type.GetTypeInfo();
            if (ti.IsPrimitive || ti.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            if (type == typeof(string))
            {
                return null;
            }

            var parameterizedType = new ParameterizedType { Type = type, Parameters = parameterOverrides };
            var mappings = GetMappings(parameterizedType);
            var hasMapping = mappings.Count > 0;
            var map = hasMapping ? mappings[mappings.Count - 1] : new TypeMap { From = type };

            if (hasMapping)
            {
                object result;
                if (map.TryGetValue(out result))
                {
                    return result;
                }

                if (map.To != type)
                {
                    return GetCore(map.To, map.DefaultParameterOverrides);
                }
            }
            
            var instance = Instantiate(type, parameterOverrides);
            if (instance != null)
            {
                map.To = instance.GetType();
            }

            map.SetValue(instance, true);

            if (!hasMapping)
            {
                mappings.Add(map);
            }

            return instance;
        }

        /// <inheritdoc />
        public IEnumerable GetAll(Type type)
        {
            lock (syncRoot)
            {
                dependencyTracker.Clear();
                return GetAllCore(type);
            }
        }

        private IEnumerable GetAllCore(Type type)
        {
            List<TypeMap> mappings;
            this.mappings.TryGetValue(new ParameterizedType { Type = type }, out mappings);

            IList result = Array.CreateInstance(type, mappings != null ? mappings.Count : 0);
            if (result.Count > 0)
            {
                object instance;
                for (var i = 0; i < mappings.Count; i++)
                {
                    var mapping = mappings[i];
                    if (mapping.TryGetValue(out instance))
                    {
                        result[i] = instance;
                    }
                    else
                    {
                        result[i] = GetCore(mappings[i].To, null);
                    }
                }
            }
            return result;
        }

        private List<TypeMap> GetMappings(ParameterizedType type)
        {
            List<TypeMap> result;
            if (!mappings.TryGetValue(type, out result))
            {
                mappings.Add(type, result = new List<TypeMap>());
            }
            return result;
        }

        private object Instantiate(Type type, object[] parameterOverrides)
        {
            if (dependencyTracker.Contains(type))
            {
                throw new ArgumentException($"Circular dependency detected while resolving type { type.FullName }.");
            }

            dependencyTracker.Add(type);

            try
            {
                var instance = InstantiateCore(type, parameterOverrides);
                if (instance != null)
                {
                    Map(instance.GetType(), instance, true);
                }

                return instance;
            }
            finally
            {
                dependencyTracker.Remove(type);
            }
        }

        private object InstantiateCore(Type type, object[] parameterOverrides)
        {
            if (type.IsArray)
            {
                return GetAllCore(type.GetElementType());
            }

            if (type.IsConstructedGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(IEnumerable<>))
                {
                    return GetAllCore(type.GenericTypeArguments[0]);
                }

                if (definition == typeof(Lazy<>))
                {
                    var func = funcFactory.MakeFunc(typeof(Func<>).MakeGenericType(type.GenericTypeArguments[0]));
                    return Activator.CreateInstance(type, func);
                }

                if (funcFactory.IsFuncDefinition(definition))
                {
                    return funcFactory.MakeFunc(type);
                }
            }

            var ti = type.GetTypeInfo();
            if (ti.IsAbstract || ti.IsInterface)
            {
                return null;
            }

            ConstructorInfo constructor = null;
            ParameterInfo[] parameters = null;

            // Find matching constructor with the biggest number of parameters
            var paramCount = -1;
            foreach (var candidate in type.GetTypeInfo().DeclaredConstructors)
            {
                var candidateParameters = candidate.GetParameters();
                if (candidateParameters.Length > paramCount)
                {
                    if (!ParameterMatches(candidateParameters, parameterOverrides))
                    {
                        continue;
                    }

                    constructor = candidate;
                    parameters = candidateParameters;
                    paramCount = candidateParameters.Length;
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
                if (parameterOverrides != null && i < parameterOverrides.Length)
                {
                    constructorParams[i] = parameterOverrides[i];
                }
                else
                {
                    constructorParams[i] = GetCore(parameters[i].ParameterType, null);
                }
            }

            return constructor.Invoke(constructorParams);
        }

        private bool ParameterMatches(ParameterInfo[] candidateParameters, object[] parameterOverrides)
        {
            if (parameterOverrides == null)
            {
                return true;
            }

            // Find best matching constructor using the input parameters
            var minLength = Math.Min(candidateParameters.Length, parameterOverrides.Length);

            for (int i = 0; i < minLength; i++)
            {
                var parameterType = candidateParameters[i].ParameterType.GetTypeInfo();
                var valueType = parameterOverrides[i]?.GetType();

                // Cannot assign null to a value type
                if (parameterType.IsValueType && valueType == null)
                {
                    return false;
                }

                // Cannot assign when type is not assignable
                if (valueType != null && !parameterType.IsAssignableFrom(valueType.GetTypeInfo()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}