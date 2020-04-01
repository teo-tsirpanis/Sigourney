# Sigourney

## What is Sigourney

Sigourney is a lightweight library that helps developers write _weavers_, programs that modify other .NET assemblies.

## What is not Sigourney

Sigourney is _not_ a replacement for [Fody](https://github.com/Fody/Fody). Fody is a more advanced and mature tool, but there are two problems with it:

* Fody has [a controversial licensing model](https://github.com/Fody/Home/blob/master/pages/licensing-patron-faq.md) where every user of it is expected to subscribe to either Open Collective or Tidelift. It is not mandatory though, and Fody is otherwise licensed under the MIT license. But some people would not use it without paying, for ethical reasons. And Sigourney is an alternative for them.

* Fody has a complicated configuration system that requires an additional `FodyWeavers.xml` file, and typically three NuGet packages (one for Fody itself, one for the weaver, and one for the attributes that control the weaver). Sigourney keeps it simple. Only one package is required to be referenced, configuration happens inside the project file, and any attributes are manually defined in the assembly.

If you are fine with using Fody, that's fine, keep using it. Some people might not be, and that's why Sigourney was created.

## How to use

_Sigourney is currently under construction. Instructions will be updated soon._

## License

Sigourney is licensed under the [MIT license](https://opensource.org/licenses/MIT), with no strings attached. The code that handles strong-named assemblies was incorporated from Fody. If you have any problem, do not strong-name your assemblies to be processed by Sigourney.

## Maintainers

* [__@teo-tsirpanis__](https://github.com/teo-tsirpanis)
