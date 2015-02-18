namespace Nine.Injection.Test
{
    using System.Collections.Generic;
    using System.Linq;

    interface IFoo { }

    class Foo : IFoo { }
    class Foo2 : IFoo { }

    class Bar
    {
        public IFoo Foo { get; private set; }
        public Bar(IFoo foo) { this.Foo = foo; }
    }

    interface IFirst { }
    class First : IFirst { }

    interface ISecond { }
    class Second : ISecond { }

    class Overloaded
    {
        public IFirst First { get; private set; }
        public ISecond Second { get; private set; }

        public Overloaded() : this(null, null) { }
        public Overloaded(IFirst first) : this(first, null) { }
        public Overloaded(IFirst first, ISecond second) { this.First = first; this.Second = second; }
    }

    class ArrayConstructor
    {
        public IFoo Foo { get; private set; }
        public IFoo[] Foos { get; private set; }
        public ArrayConstructor(IFoo foo, params IFoo[] foos) { this.Foo = foo; this.Foos = foos; }
    }

    class EnumerableConstructor
    {
        public IFoo Foo { get; private set; }
        public IFoo[] Foos { get; private set; }
        public EnumerableConstructor(IFoo foo, IEnumerable<IFoo> foos) { this.Foo = foo; this.Foos = foos.ToArray(); }
    }

    interface IPing { }
    class Ping : IPing { public Ping(IPong pong) { } }

    interface IPong { }
    class Pong : IPong { public Pong(IPing ping) { } }
}