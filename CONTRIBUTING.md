# Contributor's guide

## How to test

* Install .NET Core SDK, version 3.1
* `cd tests`
* `dotnet msbuild`

The `test.ps1` file runs the tests six times -thrice with `dotnet msbuild` and thrice with `msbuild` itself (only on Windows)- to ensure that projects using Sigourney work incrementally. Each test is made of building a test program that uses two test weavers, and then running it which verifies that it has been weaved. MSBuild binary logs are left for each test run.

Should the test projects fail loading on an IDE, try running it first.

<s>__Important:__ Locally running the tests after a change to Sigourney or one of the test weavers requires removing the packages named `Sigourney`, `testweaver-1` and `testweaver-2` from NuGet's global packages cache (its location can be retrieved with `dotnet nuget locals global-packages -l`). If the packages cannot be removed try closing any IDE and running `dotnet build-server shutdown`.

CI builds do not suffer from the above problem because each build is performed in a clean environment.</s> This problem does not exist anymore.

## Coding guidelines

* 4 spaces indentation on C#, two spaces on XML
* __No__ trailing spaces
* One newline at the end of the file
* The project's name must nowhere be placed next to the word "Weaver".
