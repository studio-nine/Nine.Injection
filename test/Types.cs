namespace Nine.Injection.Test
{
    using System;
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

    class Bar2
    {
        public IFoo Foo { get; private set; }
        public Bar2(Lazy<IFoo> foo) { this.Foo = foo.Value; }
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

    interface IPing
    {
        IPong Pong { get; }
    }

    class Ping : IPing
    {
        public IPong Pong { get; private set; }
        public Ping(IPong pong) { Pong = pong; }
    }

    interface IPong
    {
        IPing Ping { get; }
    }

    class Pong : IPong
    {
        public IPing Ping { get { return null; } }
        public Pong(IPing ping) { }
    }

    class Pong2 : IPong
    {
        Lazy<IPing> ping;
        public IPing Ping { get { return ping.Value; } }
        public Pong2(Lazy<IPing> ping) { this.ping = ping; }
    }

    class Pong3 : IPong
    {
        public IPing Ping { get; private set; }
        public Pong3(Lazy<IPing> ping) { this.Ping = ping.Value; }
    }
}