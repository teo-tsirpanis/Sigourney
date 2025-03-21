# A sample weaver

This folder contains a sample weaver powered by Sigourney, with MSBuild integration. It doesn't do anything useful, just adds one type to assert its dominance.

## The files

The weaver is made of these files:

* `Sigourney.TestWeaver1.csproj`: The project file. It defines the weaver's dependencies and instructs MSBuild to pack it with them.
* `TestWeaver1.cs`: The weaver's implementation.
* `Sigourney.TestWeaver1.props`: This file is automatically imported by NuGet at the beginning of the project. It registers the weaver to Sigourney.
* `Sigourney.TestWeaver1.targets`: This file is automatically imported by NuGet at the end of the project. It contains the MSBuild targets that invoke the weaver.

All these files are extensively commented. There is also a project named `Sigourney.TestWeaver2` which is used to verify that Sigourney works with more than one weaver in the same project. It's almost identical to this weaver, but not commented.

## How you would have used it

```xml
<ItemGroup>
  <PackageReference Include="Sigourney.TestWeaver1" Version="0.0.0-local" PrivateAssets="all" />
</ItemGroup>
```
