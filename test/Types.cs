namespace Nine.Ioc.Test
{
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
}
