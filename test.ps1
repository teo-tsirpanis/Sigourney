#! /usr/bin/env pwsh
# This little script runs Sigourney's test project many times to ensure it works repeatedly.
# It also keeps binary logs of each test run (which are kept as artifacts by CI).

function Remove-Directory-Checked {
    param ([string]$Directory)
    if (Test-Path $Directory -PathType Container) {
        Remove-Item $Directory -Recurse -Force
    }
}

$ErrorActionPreference = 'Stop'

$TestLogs = './test-logs/'
$TestProject = './tests/test.proj'
$LocalPackagePath = './tests/packages'
$LocalPackages = Get-ChildItem './tests' -Filter 'Sigourney.TestWeaver*.csproj' -Recurse | ForEach-Object { $_.FullName }

Get-ChildItem $LocalPackagePath -Filter 'Sigourney*' | ForEach-Object { Remove-Item $_.FullName -Recurse -Force }
Remove-Directory-Checked $TestLogs
# dotnet clean might fail the first time.
Remove-Item tests\**\obj\* -Recurse -Force

function Invoke-MSBuild-Test {
    param ([string]$MSBuildCommand, [string]$CommandPrefix)
    dotnet clean /v:m /nodereuse:false $TestProject
    for ($i = 1; ($i -le 3) -and ($LASTEXITCODE -eq 0); $i++) {
        $target = if ($i -eq 1) { "Clean;Test" } else { "Test" }
        $testArgs = @($CommandPrefix, $TestProject, "/v:m", "/t:$target", "/nodereuse:false", "/bl:$TestLogs$MSBuildCommand-$i.binlog")
        if ($i -eq 1) { $testArgs += "/restore" }
        & $MSBuildCommand @testArgs
    }
}

dotnet pack ./Sigourney.Shipping.slnf -o $LocalPackagePath -p:Version=0.0.0-local
$LocalPackages | ForEach-Object { dotnet pack $_ -o $LocalPackagePath }

Invoke-MSBuild-Test "dotnet" "msbuild"
if ($IsWindows -and ($LASTEXITCODE -eq 0)) { Invoke-MSBuild-Test "msbuild" "" }
