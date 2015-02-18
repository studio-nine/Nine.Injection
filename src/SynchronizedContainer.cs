namespace Nine.Injection
{
    using System;
    using System.Collections;

    class SynchronizedContainer : IContainer
    {
        private readonly object syncRoot = new object();
        private readonly IContainer container;

        public SynchronizedContainer(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            this.container = container;
        }

        public void Add(Type type, object instance)
        {
            lock (syncRoot)
            {
                container.Add(type, instance);
            }
        }

        public object Get(Type type)
        {
            lock (syncRoot)
            {
                return container.Get(type);
            }
        }

        public IEnumerable GetAll(Type type)
        {
            lock (syncRoot)
            {
                return container.GetAll(type);
            }
        }

        public void Map(Type from, Type to)
        {
            lock (syncRoot)
            {
                container.Map(from, to);
            }
        }
    }
}