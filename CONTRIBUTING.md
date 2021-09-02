# Contributor's guide

## How to test

* Install .NET Core SDK, version 3.1
* Install PowerShell
* `cd tests`
* `dotnet msbuild`

The `test.ps1` file runs the tests six times -thrice with `dotnet msbuild` and thrice with `msbuild` itself (only on Windows)- to ensure that projects using Sigourney work incrementally. Each test is made of building a test program that uses two test weavers, and then running it which verifies that it has been weaved. MSBuild binary logs are left for each test run in the `test-logs` folder.

Should the test projects fail loading on an IDE, try running it first.

## How to pack

Run the `pack.ps1` script. The packages will be placed in the `bin` folder.

## Coding guidelines

* 4 spaces indentation on C#, two spaces on XML
* __No__ trailing spaces
* One newline at the end of the file
* The project's name must nowhere be placed next to the word "Weaver".
