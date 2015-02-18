namespace Nine.Ioc.Test
{
    using System.Linq;
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
            Assert.IsType<Foo>(new Container().Add<IFoo>(new Foo()).Get<IFoo>());
        }

        [Fact]
        public void add_to_the_same_type_should_override()
        {
            Assert.IsType<Foo2>(new Container().Add<IFoo>(new Foo2()).Get<IFoo>());
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
            Assert.IsType<Foo2>(new Container().Add<IFoo>(new Foo2()).Get<Bar>().Foo);
        }

        [Fact]
        public void get_picks_the_constructor_with_most_parameters()
        {
            var instance = new Container().Map<IFirst, First>().Map<ISecond, Second>().Get<Overloaded>();
            Assert.IsType<First>(instance.First);
            Assert.IsType<Second>(instance.Second);
        }
    }
}
