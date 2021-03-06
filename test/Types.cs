﻿namespace Nine.Injection.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    interface IFoo { }
    interface IFooDerived { }
    interface IFoo2 { }

    class Foo : IFoo { }
    class Foo2 : IFoo, IFoo2 { }

    struct StructFoo : IFoo
    {
        public int Value;
    }

    class DisposableFoo : IFoo, IDisposable
    {
        public bool IsDisposed;
        public void Dispose() { IsDisposed = true; }
    }

    class WeakFoo : IFoo
    {
        public static int InstanceCount;
        public WeakFoo() { InstanceCount++; }
    }

    class WeakFoo2 : IFoo
    {
        public static int InstanceCount;
        public WeakFoo2() { InstanceCount++; }
    }

    class ChainFoo1 : IFoo
    {
        public IFoo Foo { get; private set; }
        public ChainFoo1(IFoo foo) { this.Foo = foo; }
    }

    class ChainFoo2 : IFoo
    {
        public IFoo Foo { get; private set; }
        public ChainFoo2(IFoo foo) { this.Foo = foo; }
    }

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

    class ArrayConstructor2Foo
    {
        public ArrayConstructor2Foo(IFoo foo) { }
    }

    class ArrayConstructor2
    {
        public IFoo[] Foos { get; private set; }
        public ArrayConstructor2(ArrayConstructor2Foo foo, params IFoo[] foos) { this.Foos = foos; }
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

    class PongLazy : IPong
    {
        Lazy<IPing> ping;
        public IPing Ping { get { return ping.Value; } }
        public PongLazy(Lazy<IPing> ping) { this.ping = ping; }
    }

    class PongFunc : IPong
    {
        Func<IPing> ping;
        public IPing Ping { get { return ping(); } }
        public PongFunc(Func<IPing> ping) { this.ping = ping; }
    }

    class PongLazyInConstructor : IPong
    {
        public IPing Ping { get; private set; }
        public PongLazyInConstructor(Lazy<IPing> ping) { this.Ping = ping.Value; }
    }

    interface IPerInstanceParameter
    {
        int Id { get; }
    }

    class PerInstanceParameter : IPerInstanceParameter
    {
        public int Id { get; private set; }
        public IFoo Foo;
        public PerInstanceParameter(int id, IFoo foo) { this.Id = id; this.Foo = foo; }
    }

    class PerInstanceParameter2
    {
        public object Id;
        public IFoo Foo;
        public PerInstanceParameter2(int id, IFoo foo) { this.Id = id; this.Foo = foo; }
        public PerInstanceParameter2(string id, IFoo foo) { this.Id = id; this.Foo = foo; }
        public PerInstanceParameter2(object id, IFoo foo) { this.Id = id; this.Foo = foo; }
    }

    class PerInstanceParameter3 : IPerInstanceParameter
    {
        public int Id { get; private set; }
        public IFoo Foo;
        public PerInstanceParameter3(string id, IFoo foo) { this.Id = int.Parse(id); this.Foo = foo; }
    }

    class PerInstanceParameterConsumer
    {
        private readonly PerInstanceParameter3 _p;
        public PerInstanceParameter3 Obj => _p;
        public int Id => _p.Id;
        public PerInstanceParameterConsumer(PerInstanceParameter3 p) { _p = p; }
    }

    class DependsOnPerInstanceParameter
    {
        public Func<int, PerInstanceParameter> Factory;
        public Func<int, IFoo, PerInstanceParameter> Factory2;
        public DependsOnPerInstanceParameter(
            Func<int, PerInstanceParameter> factory,
            Func<int, IFoo, PerInstanceParameter> factory2)
        {
            this.Factory = factory;
            this.Factory2 = factory2;
        }
    }

    class ConstructorWithDefaultParameter
    {
        public int Int;
        public string String;
        public Bar Bar;
        public ConstructorWithDefaultParameter(int i = 1, string s = "default", Bar bar = null)
        {
            this.Int = i;
            this.String = s;
            this.Bar = bar;
        }
    }

    class ConstructorWithDefaultStructParameter
    {
        public StructFoo Foo;
        public ConstructorWithDefaultStructParameter(StructFoo foo)
        {
            this.Foo = foo;
        }
    }

    interface IOpenGenerics<T1, T2> { }
    class OpenGenerics<T1, T2> : IOpenGenerics<T1, T2> { }

    class DependsOnOpenGenerics<T1, T2>
    {
        public int Id;
        public IOpenGenerics<T1, T2> Data;
        public DependsOnOpenGenerics(int id, IOpenGenerics<T1, T2> data) { this.Id = id; this.Data = data; }
    }

    class DependsOnClosedGenerics
    {
        public IOpenGenerics<int, bool> Data;
        public DependsOnClosedGenerics(IOpenGenerics<int, bool> data) { this.Data = data; }
    }

    class ClassWithStaticConstructor
    {
        static ClassWithStaticConstructor() { }
        public ClassWithStaticConstructor() { }
    }

    class ClassWithPrivateConstructors
    {
        public int A;
        private ClassWithPrivateConstructors() { }
        public ClassWithPrivateConstructors(int a = 10) { A = a; }
    }
}