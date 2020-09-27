# Sigourney

[![NuGet](https://img.shields.io/nuget/v/Sigourney)](https://nuget.org/packages/Sigourney)

## What is Sigourney

Sigourney is a lightweight toolkit that helps developers write weavers, tools that modify other .NET assemblies using [Mono.Cecil](https://github.com/jbevain/cecil).

## Comparing Sigourney with Fody

Sigourney is _not_ a replacement for [Fody](https://github.com/Fody/Fody). Fody is a more advanced and mature tool, but there are two problems with it:

* Fody has [an unusual licensing model](https://github.com/Fody/Home/blob/master/pages/licensing-patron-faq.md) where every user of it is expected to subscribe to either Open Collective or Tidelift. This requirement is not mandatory though, and Fody is otherwise licensed under the MIT license. But for the people (like Sigourney's author) who prefer to not use Fody at all instead of paying, Sigourney is an alternative.

* Fody has a complicated configuration system that requires an additional `FodyWeavers.xml` file, and typically three NuGet packages (one for Fody itself, one for the weaver, and one for the attributes that control the weaver). Sigourney keeps it simple and flexible for the author of the weaver. Only two packages are required to be referenced (the package with the weaver and Sigourney itself), configuration happens inside the project file, and any attributes are manually defined in the assembly.

Again, if you are fine with using Fody, that's fine, keep using it.

Sigourney can also be used as a standalone library, without hooking it to an MSBuild task.

## How to use

_Sigourney is currently under construction. Instructions will be updated soon._

## License

Sigourney is licensed under the [MIT license](https://opensource.org/licenses/MIT), with no strings attached. The code that handles strong-named assemblies was incorporated from Fody. If you have any problem, do not strong-name your assemblies while you are using Sigourney. And why are you still strong-naming your assemblies?

## Maintainers

* [__@teo-tsirpanis__](https://github.com/teo-tsirpanis)
