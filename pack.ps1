#! /usr/bin/env pwsh

$PackageOutputDir = './bin'

Remove-Item $PackageOutputDir -Recurse -Force -ErrorAction Ignore

dotnet pack src/Sigourney/Sigourney.csproj -c Release -p:ContinuousIntegrationBuild=true -o $PackageOutputDir
dotnet pack src/Sigourney.Build/Sigourney.Build.csproj -o $PackageOutputDir
