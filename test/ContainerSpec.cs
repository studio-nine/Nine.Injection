namespace Nine.Injection.Test
{
    using System;
    using System.Reflection;
    using Xunit;

    public class ContainerSpec
    {
        [Fact]
        public void map_then_get_a_type()
        {
            Assert.IsType<Foo>(new Container().Map<IFoo, Foo>().Get<IFoo>());
        }

        [Fact]
        public void map_to_the_same_type_should_override()
        {
            Assert.IsType<Foo2>(new Container().Map<IFoo, Foo>().Map<IFoo, Foo2>().Get<IFoo>());
        }

        [Fact]
        public void add_then_get_an_instance()
        {
            Assert.IsType<Foo>(new Container().Map<IFoo>(new Foo()).Get<IFoo>());
        }

        [Fact]
        public void add_to_the_same_type_should_override()
        {
            Assert.IsType<Foo2>(new Container().Map<IFoo>(new Foo2()).Get<IFoo>());
        }

        [Fact]
        public void get_from_the_same_container_returns_the_same_instance()
        {
            var container = new Container().Map<IFoo, Foo>();
            Assert.Equal(container.Get<IFoo>(), container.Get<IFoo>());
        }

        [Fact]
        public void get_from_different_containers_returns_different_instances()
        {
            Assert.NotEqual(new Container().Map<IFoo, Foo>().Get<IFoo>(),
                            new Container().Map<IFoo, Foo>().Get<IFoo>());
        }

        [Fact]
        public void object_mapped_to_different_interfaces_is_also_singleton()
        {
            var container = new Container().Map<IFoo, Foo2>().Map<IFoo2, Foo2>();
            Assert.Equal(container.Get<IFoo>(), container.Get<IFoo>());
            Assert.Equal(container.Get<IFoo>(), (IFoo)container.Get<IFoo2>());
        }

        [Fact]
        public void get_unmapped_type()
        {
            Assert.IsType<Foo>(new Container().Get<Foo>());
        }

        [Fact]
        public void get_using_constructor_injection()
        {
            Assert.IsType<Foo>(new Container().Map<IFoo, Foo>().Get<Bar>().Foo);
        }

        [Fact]
        public void get_using_constructor_injection_with_registered_instance()
        {
            Assert.IsType<Foo2>(new Container().Map<IFoo>(new Foo2()).Get<Bar>().Foo);
        }

        [Fact]
        public void get_picks_the_constructor_with_most_parameters()
        {
            var instance = new Container().Map<IFirst, First>().Map<ISecond, Second>().Get<Overloaded>();
            Assert.IsType<First>(instance.First);
            Assert.IsType<Second>(instance.Second);
        }

        [Fact]
        public void get_should_throw_on_circular_dependency()
        {
            Assert.Throws<ArgumentException>(() => new Container().Map<IPing, Ping>().Map<IPong, Pong>().Get<IPing>());
        }

        [Fact]
        public void lazy_can_resolve_circular_dependency()
        {
            var ping = new Container().Map<IPing, Ping>().Map<IPong, Pong2>().Get<IPing>();
            Assert.Equal(ping, ping.Pong.Ping);
        }

        [Fact]
        public void resolve_lazy_within_constructor_should_throw_on_circular_dependency()
        {
            Assert.Throws<TargetInvocationException>(() => new Container().Map<IPing, Ping>().Map<IPong, Pong3>().Get<IPing>());
        }

        [Fact]
        public void resolve_lazy_within_constructor_should_not_throw()
        {
            Assert.IsType<Foo>(new Container().Map<IFoo, Foo>().Get<Bar2>().Foo);
        }

        [Fact]
        public void get_all_returns_all_mapping_in_registration_order()
        {
            var instance = new Container().Map<IFoo, Foo>().Map<IFoo>(new Foo2()).Map<IFoo>(new Foo()).GetAll<IFoo>();
            Assert.Collection(instance, e => Assert.IsType<Foo>(e), e => Assert.IsType<Foo2>(e), e => Assert.IsType<Foo>(e));
        }

        [Fact]
        public void get_all_should_return_updated_collection()
        {
            var container = new Container().Map<IFoo, Foo>().Map<IFoo, Foo2>();
            var foo1 = container.GetAll<IFoo>();
            var foo2 = container.Map<IFoo>(new Foo()).Map<IFoo, Foo2>().GetAll<IFoo>();
            Assert.Collection(foo1, e => Assert.IsType<Foo>(e), e => Assert.IsType<Foo2>(e));
            Assert.Collection(foo2, e => Assert.IsType<Foo>(e), e => Assert.IsType<Foo2>(e), e => Assert.IsType<Foo>(e), e => Assert.IsType<Foo2>(e));
        }

        [Fact]
        public void get_should_inject_array_constructor_parameter()
        {
            var instance = new Container().Map<IFoo, Foo>().Map<IFoo, Foo2>().Get<ArrayConstructor>();
            Assert.NotNull(instance);
            Assert.IsType<Foo2>(instance.Foo);
            Assert.Collection(instance.Foos, e => Assert.IsType<Foo>(e), e => Assert.Equal(instance.Foo, e));
        }

        [Fact]
        public void get_should_inject_enumerable_constructor_parameter()
        {
            var instance = new Container().Map<IFoo, Foo>().Map<IFoo, Foo2>().Get<EnumerableConstructor>();
            Assert.NotNull(instance);
            Assert.IsType<Foo2>(instance.Foo);
            Assert.Collection(instance.Foos, e => Assert.IsType<Foo>(e), e => Assert.Equal(instance.Foo, e));
        }
    }
}
