namespace Nine.Injection.Test
{
    using System;
    using System.Linq;
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
            var foo = new Foo();
            Assert.Equal(foo, new Container().Map<IFoo>(foo).Get<IFoo>());
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
        public void throw_when_the_container_is_freezed()
        {
            var container = new Container().Freeze();
            Assert.Throws<InvalidOperationException>(() => container.Map<IFoo, Foo>());
        }

        [Fact]
        public void get_unmapped_type()
        {
            Assert.IsType<Foo>(new Container().Get<Foo>());
        }

        [Fact]
        public void container_itself_it_mapped_automatically()
        {
            var container = new Container();
            Assert.Equal(container, container.Get<IContainer>());
            Assert.Equal(container, container.Get<Container>());
        }

        [Fact]
        public void get_respect_parameter_default_value()
        {
            Assert.Equal("default", new Container().Get<ConstructorWithDefaultParameter>().String);
        }

        [Fact]
        public void primitive_types_are_mapped_to_the_default_value()
        {
            var container = new Container();

            Assert.Equal(0, container.Get<int>());
            Assert.Equal(0ul, container.Get<ulong>());
            Assert.Equal(0.0, container.Get<double>());
            Assert.Equal(null, container.Get<string>());
            Assert.Equal(default(DateTime), container.Get<DateTime>());
            Assert.Equal(default(TimeSpan), container.Get<TimeSpan>());
        }

        [Fact]
        public void get_using_constructor_injection()
        {
            Assert.IsType<Foo>(new Container().Map<IFoo, Foo>().Get<Bar>().Foo);
        }

        [Fact]
        public void get_using_constructor_injection_with_registered_instance()
        {
            var foo = new Foo2();
            Assert.Equal(foo, new Container().Map<IFoo>(foo).Get<Bar>().Foo);
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
            Assert.Throws<TargetInvocationException>(() => new Container().Map<IPing, Ping>().Map<IPong, Pong>().Get<IPing>());
        }

        [Fact]
        public void lazy_can_resolve_circular_dependency()
        {
            var ping = new Container().Map<IPing, Ping>().Map<IPong, PongLazy>().Get<IPing>();
            Assert.Equal(ping, ping.Pong.Ping);
        }

        [Fact]
        public void func_can_resolve_circular_dependency()
        {
            var ping = new Container().Map<IPing, Ping>().Map<IPong, PongFunc>().Get<IPing>();
            Assert.Equal(ping, ping.Pong.Ping);
        }

        [Fact]
        public void can_override_the_default_func_implementation_provided_by_the_container()
        {
            var ping1 = new Ping(null);
            var ping = new Container().Map<IPing, Ping>().Map<IPong, PongFunc>().Map(new Func<IPing>(() => ping1)).Get<IPing>();
            Assert.Equal(ping1, ping.Pong.Ping);
            Assert.NotEqual(ping1, ping);
        }

        [Fact]
        public void resolve_lazy_within_constructor_should_throw_on_circular_dependency()
        {
            Assert.Throws<TargetInvocationException>(() => new Container().Map<IPing, Ping>().Map<IPong, PongLazyInConstructor>().Get<IPing>());
        }

        [Fact]
        public void resolve_lazy_within_constructor_should_not_throw()
        {
            Assert.IsType<Foo>(new Container().Map<IFoo, Foo>().Get<Bar2>().Foo);
        }

        [Fact]
        public void get_all_returns_all_mapping_in_registration_order()
        {
            var foos = new IFoo[] { new Foo2(), new Foo() };
            var instance = new Container().Map<IFoo, Foo>().Map(foos[0]).Map(foos[1]).GetAll<IFoo>();
            Assert.IsType<Foo>(instance.First());
            Assert.NotEqual(instance.First(), foos[1]);
            Assert.Equal(foos, instance.Skip(1));
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

        [Fact]
        public void container_tracks_created_objects_using_weak_references()
        {
            var container = new Container().Map<IFoo, WeakFoo>();
            var instance = container.Get<IFoo>();
            Assert.Equal(1, WeakFoo.InstanceCount);
            instance = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            instance = container.Get<IFoo>();
            Assert.Equal(2, WeakFoo.InstanceCount);
        }

        [Fact]
        public void container_tracks_explicitly_mapped_objects_using_strong_references()
        {
            var container = new Container().Map<IFoo>(new WeakFoo2());
            var instance = container.Get<IFoo>();
            Assert.Equal(1, WeakFoo2.InstanceCount);
            instance = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            instance = container.Get<IFoo>();
            Assert.Equal(1, WeakFoo2.InstanceCount);
        }

        [Fact]
        public void get_object_with_parameter_override()
        {
            var container = new Container().Map<IFoo, Foo>();
            var instances = new[]
            {
                container.Get<PerInstanceParameter>(),
                container.Get<PerInstanceParameter>(1),
                container.Get<PerInstanceParameter>(2),
            };

            Assert.All(instances, i => Assert.IsType<Foo>(i.Foo));
            Assert.Equal(new[] { 0, 1, 2 }, instances.Select(i => i.Id));
        }

        [Fact]
        public void get_object_with_parameter_override_should_use_the_best_matching_constructor()
        {
            var container = new Container().Map<IFoo, Foo>();
            var instances = new[]
            {
                container.Get<PerInstanceParameter2>(1),
                container.Get<PerInstanceParameter2>("2"),
            };

            Assert.All(instances, i => Assert.IsType<Foo>(i.Foo));
            Assert.Equal(new object[] { 1, "2", }, instances.Select(i => i.Id));
        }

        [Fact]
        public void get_object_with_multiple_parameter_overrides()
        {
            var a = new Foo();
            var container = new Container().Map<IFoo, Foo>();

            Assert.Equal(a, container.Get<PerInstanceParameter>(1, a).Foo);
            Assert.Null(container.Get<PerInstanceParameter>(1, null).Foo);
        }

        [Fact]
        public void returns_the_same_instance_when_parameters_equals()
        {
            var a = new Foo();
            var container = new Container().Map<IFoo, Foo>();
            Assert.Equal(container.Get<PerInstanceParameter>(1), container.Get<PerInstanceParameter>(1));
            Assert.Equal(container.Get<PerInstanceParameter>(1, a), container.Get<PerInstanceParameter>(1, a));
            Assert.NotEqual(container.Get<PerInstanceParameter>(1, a), container.Get<PerInstanceParameter>(1));
        }

        [Fact]
        public void use_func_as_the_factory_to_create_instance_with_custom_parameters()
        {
            var a = new Foo();
            var factory = new Container().Map<IFoo, Foo>().Get<DependsOnPerInstanceParameter>();
            Assert.Equal(10, factory.Factory(10).Id);
            Assert.Equal(a, factory.Factory2(10, a).Foo);
        }

        [Fact]
        public void it_should_only_inject_func_when_the_return_type_contains_a_matching_constructor()
        {
            Assert.NotNull(new Container().Get<Func<int, PerInstanceParameter2>>());
            Assert.NotNull(new Container().Get<Func<int, IFoo, PerInstanceParameter2>>());
            Assert.NotNull(new Container().Get<Func<string, IFoo, PerInstanceParameter2>>());

            Assert.Null(new Container().Get<Func<long, IFoo>>());
            Assert.Null(new Container().Get<Func<long, Foo>>());

            Assert.Null(new Container().Get<Func<int, IFooDerived, PerInstanceParameter2>>());
            Assert.Null(new Container().Get<Func<long, PerInstanceParameter2>>());
            Assert.Null(new Container().Get<Func<long, IFoo2, PerInstanceParameter2>>());
        }

        [Fact]
        public void map_object_with_parameter_override()
        {
            Assert.Equal(1234, new Container()
                .Map<IPerInstanceParameter, PerInstanceParameter>(1234)
                .Get<IPerInstanceParameter>().Id);

            Assert.Equal(1234, new Container()
                .Map<IPerInstanceParameter, PerInstanceParameter>(1234)
                .Get<IPerInstanceParameter[]>()[0].Id);
        }
    }
}
