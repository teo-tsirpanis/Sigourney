# A sample weaver

This folder contains a sample weaver powered by Sigourney, with MSBuild integration.

## The files

The weaver is made of these files:

* `testweaver-1.csproj`: The project file. It defines the weaver's dependencies and instructs MSBuild to pack it with them.
* `TestWeaver1.cs`: The weaver's implementation.
* `testweaver-1.props`: This file is automatically imported by NuGet at the beginning of the project. It registers the weaver to Sigourney.
*
  `testweaver-1.Sigourney.targets`: This file contains the MSBuild targets that invoke the weaver.

  __Important:__ This file is deliberately not named `testweaver-1.targets`. It __MUST NOT__ be automatically imported by NuGet. Instead, it is registered in `testweaver-1.props` and imported by Sigourney in the right place.

All these files are extensively commented. There is also a project named `testweaver-2` which is used to verify that Sigourney works with more than one weaver in the same project. It's almost identical to this weaver, but not commented at all.
