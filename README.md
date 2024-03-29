# Sigourney

[![NuGet](https://img.shields.io/nuget/v/Sigourney)][nuget]
[![CI build status](https://github.com/teo-tsirpanis/Sigourney/actions/workflows/ci.yml/badge.svg?branch=mainstream&event=push)](https://github.com/teo-tsirpanis/Sigourney/actions/workflows/ci.yml)

Sigourney is a lightweight toolkit that helps developers write weavers, tools that modify other .NET assemblies using [Mono.Cecil][cecil].

## Projects using Sigourney

At the moment, Sigourney is known to be used by two projects, both developed by Sigourney's author.

* [Covarsky][covarsky], a tool that brings co(ntra)variance in languages that don't support it like F#.
* [Farkle][farkle], an LALR parsing library that uses Sigourney for [its grammar precompiler][farkle-precompiler].

If your project uses Sigourney, feel free to open a pull request to add it to the list. It would really help with understanding if and how third parties are using it, and managing breaking changes.

## Why use Sigourney

### Comparing Sigourney with Fody

When the words ".NET assembly" and "weaver" appear in the same sentence, most developers think of [Fody][fody].

Sigourney is a competitor but not a _replacement_ for Fody. Fody is a more advanced, mature and battle-tested tool, but there are two problems with it:

* Fody has [an unusual licensing model][fody-licensing] where every user of it is expected to subscribe to either Open Collective or Tidelift. This requirement is not mandatory though, and Fody is otherwise licensed under the MIT license. But for the people (like Sigourney's author) who prefer to not use Fody at all instead of paying, Sigourney is an alternative.

* Fody has a complicated configuration system that requires an additional `FodyWeavers.xml` file, an XML schema for that file, and typically _three_ NuGet packages: one for Fody itself, one for the weaver, and one for the attributes that control its behavior. Sigourney keeps it simple and flexible. Only two packages are required to be referenced (the package with the weaver and Sigourney itself), configuration usually happens inside the project file, with any attributes being manually defined in the assembly to be weaved.

Fody on the other hand has a much larger community and [variety of weavers developed with it][fody-weavers], whereas Sigourney is a relatively new project whose community and variety of weavers are nearly nonexistent.

Sigourney can also be used as a standalone library without hooking it to MSBuild; something that Fody cannot do.

### Comparing Sigourney with Mono.Cecil

In its essence, Sigourney is a thin layer over Mono.Cecil (Fody is arguably thicker). Using Sigourney is better than directly using Mono.Cecil because of facilities Sigourney provides that you would otherwise implement yourself, like the following:

* Assemblies weaved by Sigourney are marked with a type having a name like `ProcessedByMyAwesomeWeaver`. If your awesome weaver attempts to weave the same assembly more than once, Sigourney will do nothing.
* Sigourney provides easy MSBuild integration of your weavers, allowing them to run when you build your project, without any extra steps. More on that right below.
* Sigourney supports strong-named assemblies easily ([with a caveat](#known-issues)), abstracting away most of the logic behind finding the `.snk` files.
* Sigourney automatically updates the debug symbols of the assemblies, allowing them to still be debugged.

## How to use

### Using Sigourney with MSBuild

In most cases, using a weaver powered by Sigourney is as easy as installing a NuGet package. Consult the documentation of that package for more details.

Sigourney has a particular pattern for creating MSBuild-based weavers that can coexist with others in the same project and support incremental building. To learn how to create a weaver based on that pattern, this repository has a sample project in [`tests/testweaver-1`][testweaver1].

To easily disable all weavers that were implemented according to the standard pattern, add the following line inside a `PropertyGroup` in your project file:

```xml
<SigourneyEnable>false</SigourneyEnable>
```

### Using Sigourney as a standalone library

Sigourney can also be used as a standalone library, as part of a bigger program that weaves .NET assemblies. Simply install [Sigourney's NuGet package][nuget] and use it like this:

```csharp
using Mono.Cecil;
using Serilog.Core;
using Sigourney;

public class Test
{
    public bool DoWeave(AssemblyDefinition asm)
    {
        for (var t in asm.MainModule.Types)
        {
            // Do what you want with each type of the assembly.
        }
        return true;
    }

    public static void Main()
    {
        Weaver.Weave("MyAssembly.dll", "MyAssembly.weaved.dll", DoWeave, Logger.None, null, "MyAwesomeWeaver");
    }
}
```

To learn more, consult the documentation of the [`Weaver`][weaver-class] class.

## Supported versions policy

__TL;DR:__ Since all known weavers are first-party, backwards compatibility is not a top priority of Sigourney. Upgrade your SDK often. Breaking changes are avoided but inevitable. Expect them in minor releases but not in patch releases.

Sigourney is a .NET Standard 2.0 library, meaning that it will work in both .NET Framework and .NET Core-based editions of MSBuild (the former are used with the `msbuild` command or on Visual Studio for Windows, and the latter when using modern `dotnet` SDK commands). Unless an assembly with a weaver targets .NET Standard too, its author has to load the correct assembly using MSBuild's `MSBuildRuntimeType` property.

Weavers using Sigourney do not support NuGet clients older than 5.0, which was released with Visual Studio 2019. The [Paket](https://fsprojects.github.io/Paket) package manager is not tested.

Because Mono.Cecil treats assemblies in a framework-agnostic way, Sigourney should work with any framework version supported by your SDK.

No MSBuild version is explicitly supported or unsupported, but Sigourney is only tested against the latest one. Earlier ones might be supported, or maybe not.

Sigourney is tested with SDK-style projects only. Legacy .NET Framework projects (the big, unreadable ones) are not known whether they work or not.

Like Mono.Cecil, Sigourney's version number will most likely stick in the `0.x.y` range. Patch releases will not break code, although they might upgrade libraries. Minor releases are more likely to break stuff but such impact will be attempted to be kept at a minimum.

## Known issues

* Strong-naming assemblies is not supported when you build your project using a .NET Core-based edition of MSBuild.

*
    When you build a project with many weavers using a .NET Framework-based edition of MSBuild, each weaver's dependencies are not isolated. For example, if your project uses two weavers and each of them uses a different version of Sigourney, MSBuild will only use the version of Sigourney that the weaver that ran first used. This is an inherent limitation of the .NET Framework whose fix is not trivial and not planned for Sigourney.

    To work around this, ensure that all weavers use the same version of Sigourney, or use a .NET Core-based edition of MSBuild. If you can't do that because you are are using Visual Studio on Windows, [please upvote this feedback item](https://developercommunity.visualstudio.com/t/Allow-building-SDK-style-projects-with-t/1331985).

## License

Sigourney is licensed under the [MIT license][mit], with no strings attached.

The code that handles strong-named assemblies was originally copied from Fody. If you have any problem with this, do not strong-name your assemblies that are weaved by Sigourney. And why are you still strong-naming your assemblies?

## Maintainers

* [__@teo-tsirpanis__](https://github.com/teo-tsirpanis)

[nuget]: https://nuget.org/packages/Sigourney
[ci]: https://github.com/teo-tsirpanis/Sigourney/actions
[cecil]: https://github.com/jbevain/cecil
[farkle]: https://github.com/teo-tsirpanis/Farkle
[farkle-precompiler]: https://teo-tsirpanis.github.io/Farkle/the-precompiler.html
[covarsky]: https://github.com/teo-tsirpanis/Covarsky
[fody]: https://github.com/Fody/Fody
[fody-licensing]: https://github.com/Fody/Home/blob/master/pages/licensing-patron-faq.md
[fody-weavers]: https://github.com/Fody/Home/blob/master/pages/addins.md
[testweaver1]: https://github.com/teo-tsirpanis/Sigourney/tree/mainstream/tests/testweaver-1
[weaver-class]: https://github.com/teo-tsirpanis/Sigourney/tree/mainstream/src/Sigourney/Weaver.cs
[mit]: https://opensource.org/licenses/MIT
