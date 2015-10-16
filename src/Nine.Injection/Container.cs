﻿namespace Nine.Injection
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
#if !PCL
        , IServiceProvider
#endif
    {
        private bool _freezed;
        private readonly object _syncRoot = new object();
        private readonly FuncFactory _funcFactory;
        private readonly Dictionary<ParameterizedType, List<TypeMap>> _mappings = new Dictionary<ParameterizedType, List<TypeMap>>();
        private readonly HashSet<Type> _dependencyTracker = new HashSet<Type>();
        private readonly Stack<Type> _instantiationStack = new Stack<Type>();
        private readonly Lazy<Dictionary<Type, PropertyInfo>> _lazyValueProperties = new Lazy<Dictionary<Type, PropertyInfo>>(() => new Dictionary<Type, PropertyInfo>(), isThreadSafe: false);

        /// <inheritdoc />
        public Container()
        {
            _funcFactory = new FuncFactory(this);
            Map(typeof(IContainer), this);
#if !PCL
            Map(typeof(IServiceProvider), this);
#endif
        }

        /// <summary>
        /// Gets the type mappings managed by this container.
        /// </summary>
        public IEnumerable<TypeMap> Mappings
        {
            get { return _mappings.SelectMany(m => m.Value).Where(m => m.To != null); }
        }

        /// <summary>
        /// Gets or sets the equality comparer to compare the equality of parameter objects.
        /// </summary>
        public IEqualityComparer<object> EqualityComparer { get; set; }

        /// <summary>
        /// Freezes this container and returns the freezed (this) instance.
        /// </summary>
        /// <returns></returns>
        public IContainer Freeze()
        {
            _freezed = true;
            return this;
        }

        /// <inheritdoc />
        public void Map(Type from, Type to) => Map(from, to, null);

        /// <inheritdoc />
        public void Map(Type from, Type to, params object[] parameterOverrides)
        {
            if (_freezed)
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

            lock (_syncRoot)
            {
                GetMappings(new ParameterizedType(from, null, EqualityComparer)).Add(new TypeMap
                {
                    From = from,
                    To = to,
                    IsExplicit = true,
                    DefaultParameterOverrides = parameterOverrides
                });

                if (from != to)
                {
                    GetMappings(new ParameterizedType(to, null, EqualityComparer)).Add(new TypeMap
                    {
                        From = to,
                        To = to,
                        IsExplicit = true,
                        DefaultParameterOverrides = parameterOverrides
                    });
                }

                if (parameterOverrides != null && parameterOverrides.Length > 0)
                {
                    GetMappings(new ParameterizedType(from, parameterOverrides?.ToArray(), EqualityComparer)).Add(new TypeMap
                    {
                        From = from,
                        To = to,
                        IsExplicit = true,
                        DefaultParameterOverrides = parameterOverrides
                    });

                    if (from != to)
                    {
                        GetMappings(new ParameterizedType(to, parameterOverrides?.ToArray(), EqualityComparer)).Add(new TypeMap
                        {
                            From = to,
                            To = to,
                            IsExplicit = true,
                            DefaultParameterOverrides = parameterOverrides
                        });
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Map(Type type, object instance)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_freezed)
            {
                throw new InvalidOperationException("Cannot map a type when the container is freezed");
            }

            lock (_syncRoot)
            {
                Map(type, instance, false);

                if (instance != null && type != instance.GetType())
                {
                    Map(instance.GetType(), instance, false);
                }
            }
        }

        private void Map(Type type, object instance, bool weak)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var mapping = new TypeMap { From = type, IsExplicit = true };
            mapping.SetValue(instance, weak);
            if (instance != null)
            {
                mapping.To = instance.GetType();
            }

            GetMappings(new ParameterizedType(type, null, EqualityComparer)).Add(mapping);
        }

        /// <inheritdoc />
        public object Get(Type type)
        {
            lock (_syncRoot)
            {
                _dependencyTracker.Clear();
                return GetCore(type, null).Object;
            }
        }

        /// <inheritdoc />
        public object Get(Type type, params object[] parameterOverrides)
        {
            lock (_syncRoot)
            {
                _dependencyTracker.Clear();
                return GetCore(type, parameterOverrides).Object;
            }
        }

        private GetResult GetCore(Type type, object[] parameterOverrides)
        {
            var parameterizedType = new ParameterizedType(type, parameterOverrides, EqualityComparer);
            var mappings = GetMappings(parameterizedType);
            var hasMapping = mappings.Count > 0;
            var map = hasMapping ? mappings[mappings.Count - 1] : new TypeMap { From = type };

            if (hasMapping)
            {
                object result;
                if (map.TryGetValue(out result))
                {
                    return new GetResult { Object = result, IsExplicitlyMapped = map.IsExplicit };
                }

                if (map.To != type && map.To != null)
                {
                    return GetCore(map.To, map.DefaultParameterOverrides);
                }

                parameterOverrides = parameterOverrides ?? map.DefaultParameterOverrides;
            }

            if (parameterOverrides == null || parameterOverrides.Length == 0)
            {
                var lazyMappings = GetMappings(new ParameterizedType(typeof(Lazy<>).MakeGenericType(type), null, EqualityComparer));
                if (lazyMappings.Count > 0)
                {
                    object result;
                    var lazyMap = lazyMappings[lazyMappings.Count - 1];
                    if (lazyMap.TryGetValue(out result) && result != null)
                    {
                        return new GetResult { Object = GetLazyValue(result), IsExplicitlyMapped = lazyMap.IsExplicit };
                    }
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

            return new GetResult { Object = instance, IsExplicitlyMapped = map.IsExplicit };
        }

        /// <inheritdoc />
        public IEnumerable GetAll(Type type)
        {
            lock (_syncRoot)
            {
                _dependencyTracker.Clear();
                return GetAllCore(type);
            }
        }

        private IEnumerable GetAllCore(Type type)
        {
            List<TypeMap> mappings;
            this._mappings.TryGetValue(new ParameterizedType(type, null, EqualityComparer), out mappings);

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
                        result[i] = GetCore(mappings[i].To, mappings[i].DefaultParameterOverrides).Object;
                    }
                }
            }
            return result;
        }

        private List<TypeMap> GetMappings(ParameterizedType type)
        {
            List<TypeMap> result;
            if (!_mappings.TryGetValue(type, out result))
            {
                _mappings.Add(type, result = new List<TypeMap>());
            }
            return result;
        }

        private object Instantiate(Type type, object[] parameterOverrides)
        {
            if (_dependencyTracker.Contains(type))
            {
                throw new ArgumentException($"Circular dependency detected while resolving type { type.FullName }.");
            }

            _dependencyTracker.Add(type);
            _instantiationStack.Push(type);

            try
            {
                var instance = InstantiateCore(type, parameterOverrides);
                if (instance != null)
                {
                    Map(instance.GetType(), instance, true);
                }

                return instance;
            }
            catch (Exception e)
            {
                var path = string.Join(" -> ", _instantiationStack.Select(t => t.FullName).Reverse());
                var exceptionText = $"Error instantiating { type.FullName }, please consider the following constructor path: { path }";
                throw new TargetInvocationException(exceptionText, e);
            }
            finally
            {
                _dependencyTracker.Remove(type);
                _instantiationStack.Pop();
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

                // Check for a registered generic type definition
                var mappings = GetMappings(new ParameterizedType { Type = definition, Parameters = parameterOverrides });
                if (mappings.Count > 0)
                {
                    var map = mappings[mappings.Count - 1];
                    if (map.To != definition)
                    {
                        return InstantiateCore(map.To.MakeGenericType(type.GetTypeInfo().GenericTypeArguments), parameterOverrides);
                    }
                }

                // Handle well known generic types
                if (definition == typeof(IEnumerable<>))
                {
                    return GetAllCore(type.GenericTypeArguments[0]);
                }

                if (definition == typeof(Lazy<>))
                {
                    var func = _funcFactory.MakeFunc(typeof(Func<>).MakeGenericType(type.GenericTypeArguments[0]));
                    return Activator.CreateInstance(type, func);
                }

                if (_funcFactory.IsFuncDefinition(definition))
                {
                    var arguments = type.GenericTypeArguments;
                    var funcReturnType = arguments.Last().GetTypeInfo();
                    if (arguments.Length == 1 || MatchConstructor(funcReturnType, arguments, arguments.Length - 1, true) != null)
                    {
                        return _funcFactory.MakeFunc(type);
                    }
                }
            }

            var ti = type.GetTypeInfo();
            if (ti.IsAbstract || ti.IsInterface || typeof(Delegate).GetTypeInfo().IsAssignableFrom(ti))
            {
                return null;
            }

            if (parameterOverrides == null || parameterOverrides.Length == 0)
            {
                if (ti.IsPrimitive || ti.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }

                if (type == typeof(string))
                {
                    return null;
                }
            }

            var constructor = MatchConstructor(ti, parameterOverrides);
            if (constructor == null)
            {
                return null;
            }

            var parameters = constructor.GetParameters();
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
                    var parameterType = parameters[i].ParameterType;
                    var getResult = GetCore(parameterType, null);
                    if (getResult.IsExplicitlyMapped || !parameters[i].HasDefaultValue)
                    {
                        constructorParams[i] = getResult.Object;
                    }
                    else
                    {
                        constructorParams[i] = parameters[i].DefaultValue;
                    }
                }
            }

            return constructor.Invoke(constructorParams);
        }

        private ConstructorInfo MatchConstructor(TypeInfo type, object[] parameterOverrides)
        {
            if (parameterOverrides == null || parameterOverrides.Length <= 0)
            {
                return MatchConstructor(type, null, 0, false);
            }

            var parameterTypes = new Type[parameterOverrides.Length];
            for (var i = 0; i < parameterOverrides.Length; i++)
            {
                parameterTypes[i] = parameterOverrides[i]?.GetType();
            }

            return MatchConstructor(type, parameterTypes, parameterTypes.Length, false);
        }

        private ConstructorInfo MatchConstructor(TypeInfo type, Type[] parameterTypes, int length, bool strict)
        {
            ConstructorInfo constructor = null;
            ParameterInfo[] parameters = null;

            // Find the best matching constructor with the biggest number of parameters
            var paramCount = -1;
            foreach (var candidate in type.DeclaredConstructors)
            {
                if (candidate.IsStatic || !candidate.IsPublic)
                {
                    continue;
                }

                var candidateParameters = candidate.GetParameters();
                if (candidateParameters.Length > paramCount)
                {
                    if (!MatchParameters(candidateParameters, parameterTypes, length, strict))
                    {
                        continue;
                    }

                    constructor = candidate;
                    parameters = candidateParameters;
                    paramCount = candidateParameters.Length;
                }
            }

            return constructor;
        }

        private bool MatchParameters(ParameterInfo[] candidateParameters, Type[] parameterTypes, int length, bool strict)
        {
            if (strict && length > candidateParameters.Length)
            {
                return false;
            }

            if (parameterTypes == null || length <= 0)
            {
                return true;
            }

            // Find best matching constructor using the input parameters
            var minLength = Math.Min(candidateParameters.Length, length);

            for (int i = 0; i < minLength; i++)
            {
                var parameterType = candidateParameters[i].ParameterType.GetTypeInfo();
                var valueType = parameterTypes[i];

                // Cannot assign null to a value type
                if (parameterType.IsValueType && valueType == null)
                {
                    return false;
                }

                // Cannot assign when type is not assignable
                if (valueType == null)
                {
                    continue;
                }

                if (!strict && !parameterType.IsAssignableFrom(valueType.GetTypeInfo()))
                {
                    return false;
                }

                if (strict && parameterType != valueType.GetTypeInfo())
                {
                    return false;
                }
            }

            return true;
        }

        private object GetLazyValue(object lazy)
        {
            PropertyInfo pi;

            var type = lazy.GetType();

            if (!_lazyValueProperties.Value.TryGetValue(type, out pi))
            {
                _lazyValueProperties.Value[type] = pi = type.GetTypeInfo().DeclaredProperties.First(p => p.Name == "Value");
            }

            return pi.GetValue(lazy);
        }

#if !PCL
        public object GetService(Type serviceType) => Get(serviceType);
#endif

        struct GetResult
        {
            public object Object;

            public bool IsExplicitlyMapped;

            public bool HasResult => IsExplicitlyMapped || Object != null;
        }
    }
}