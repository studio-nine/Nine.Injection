# Nine.Injection [![Build status](https://ci.appveyor.com/api/projects/status/k22p5qd8aumwy4xt)](https://ci.appveyor.com/project/yufeih/nine-injection)
Nine.Injection is a portable lightweight constructor injection library for singleton objects. 

## Highlights
- **Portable:** Runs on Windows, Windows Phone 8, Windows Store, iOS, Android, etc.
- **Always Singleton:** Objects registered and resolved by the container are always singletons.
- **Constructor Injection:** Dependencies are expressed and injected by constructors only.

## Installation
You can get Nine.Injection using NuGet:
```
Install-Package Nine.Injection
```
## Getting Started
Dependencies are maintained by the ``Container``
```c#
var container = new Container();
```
To map concrete implementations for an interface:
```c#
container.Map<IFoo, Foo>().Map<IFoo, Foo2>();
```
To map an existing instance for an interface:
```c#
container.Map<IFoo>(new Foo()).Map<IFoo>(new Foo2());
```
To map all public types that implements a specific interface:
```c#
container.MapAll<IFoo>(Assembly.GetExecutingAssembly());
```
To get the concrete implementation of an interface:
```c#
container.Get<IFoo>(); // Returns the last registered IFoo instance
```
To get all implementations that implements an interface:
```c#
container.GetAll<IFoo>(); // Returns all instances that implements IFoo
```
Objects get from the container are always singletons:
```c#
container.Map<IFoo, Foo>().Map<IFoo2, Foo>();
Assert.Equals(container.Get<IFoo>(), container.Get<IFoo>());
Assert.Equals(container.Get<IFoo>(), container.Get<IFoo2>());
```
## Expressing Dependencies
Nine.Injection uses constructors to find dependencies:
```c#
public class UserService : IService
{
    public UserService(IStorage storage) { ... }
}
```
To instantiate `UserService`, a `IStorage` is resolved from the container and passed to the constructor of `UserService`.

If there are multiple arguments specified in the constructor, the one with the most arguments is used:
```c#
public UserService(IStorage storage)
public UserService(IStorage storage, ILogger logger) // This one is used
```
To inject all implementations of a given interface:
```c#
public ServiceContainer(params IService[] services)
public ServiceContainer(IEnumerable<IService> services)
```
In case of circular dependency, `Lazy<>` can be used to break the cyclic reference.
```c#
public UserService(Lazy<IStorage> storage)
```

## Want to Learn More?
See the [test specs](https://github.com/studio-nine/Nine.Injection/blob/master/test/ContainerSpec.cs) to learn more about the behavior of Nine.Injection.
