namespace Nine.Injection
{
    using System;
    using System.Linq;
    using System.Reflection;

    class FuncFactory
    {
        private readonly IContainer _container;
        private readonly MethodInfo[] _methods;

        public FuncFactory(IContainer container)
        {
            _container = container;
            _methods = typeof(FuncFactory).GetRuntimeMethods().ToArray();
            _methods = Enumerable.Range(0, 8).Select(i => _methods.First(m => m.Name == "Get" + i)).ToArray();
        }

        public Func<T> Get0<T>() => new Func<T>(() => (T)_container.Get(typeof(T)));
        public Func<T1, T2> Get1<T1, T2>() => new Func<T1, T2>((t1) => (T2)_container.Get(typeof(T2), t1));
        public Func<T1, T2, T3> Get2<T1, T2, T3>() => new Func<T1, T2, T3>((t1, t2) => (T3)_container.Get(typeof(T3), t1, t2));
        public Func<T1, T2, T3, T4> Get3<T1, T2, T3, T4>() => new Func<T1, T2, T3, T4>((t1, t2, t3) => (T4)_container.Get(typeof(T4), t1, t2, t3));
        public Func<T1, T2, T3, T4, T5> Get4<T1, T2, T3, T4, T5>() => new Func<T1, T2, T3, T4, T5>((t1, t2, t3, t4) => (T5)_container.Get(typeof(T5), t1, t2, t3, t4));
        public Func<T1, T2, T3, T4, T5, T6> Get5<T1, T2, T3, T4, T5, T6>() => new Func<T1, T2, T3, T4, T5, T6>((t1, t2, t3, t4, t5) => (T6)_container.Get(typeof(T6), t1, t2, t3, t4, t5));
        public Func<T1, T2, T3, T4, T5, T6, T7> Get6<T1, T2, T3, T4, T5, T6, T7>() => new Func<T1, T2, T3, T4, T5, T6, T7>((t1, t2, t3, t4, t5, t6) => (T7)_container.Get(typeof(T7), t1, t2, t3, t4, t5, t6));
        public Func<T1, T2, T3, T4, T5, T6, T7, T8> Get7<T1, T2, T3, T4, T5, T6, T7, T8>() => new Func<T1, T2, T3, T4, T5, T6, T7, T8>((t1, t2, t3, t4, t5, t6, t7) => (T8)_container.Get(typeof(T8), t1, t2, t3, t4, t5, t6, t7));
        
        public bool IsFuncDefinition(Type type)
        {
            return type == typeof(Func<>) ||
                   type == typeof(Func<,>) ||
                   type == typeof(Func<,,>) ||
                   type == typeof(Func<,,,>) ||
                   type == typeof(Func<,,,,>) ||
                   type == typeof(Func<,,,,,>) ||
                   type == typeof(Func<,,,,,,>) ||
                   type == typeof(Func<,,,,,,,>) ||
                   type == typeof(Func<,,,,,,,,>);
        }

        public object MakeFunc(Type funcType)
        {
            var funcTypes = funcType.GenericTypeArguments;
            if (funcTypes.Length > _methods.Length)
            {
                return null;
            }
            return _methods[funcTypes.Length - 1].MakeGenericMethod(funcTypes).Invoke(this, null);
        }
    }
}