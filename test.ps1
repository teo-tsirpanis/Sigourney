#! /usr/bin/env pwsh
# This little script runs Sigourney's test project thrice to ensure it works repeatedly.
# It also keeps binary logs of each test run (which are kept as artifacts by AppVeyor).

function DotnetClean {dotnet clean /v:m /nodereuse:false $TestProject}
function Remove-Directory-Checked {
    param ([string]$Directory)
    if (Test-Path $Directory -PathType Container) {
        Remove-Item $Directory -Recurse -Force
    }
}

$TestLogs = './test-logs/'
$TestProject = './tests/test.proj'

Remove-Directory-Checked $TestLogs
# dotnet clean might fail the first time.
Remove-Item tests\**\obj\* -Recurse -Force

function Invoke-MSBuild-Test {
    param ([string]$MSBuildCommand, [string]$CommandPrefix)
    DotnetClean
    for ($i = 1; ($i -le 3) -and ($LASTEXITCODE -eq 0); $i++) {
        & $MSBuildCommand ($CommandPrefix, $TestProject, "/v:m", "/p:TestExecutionNumber=$i", "/nodereuse:false", "/bl:$TestLogs$MSBuildCommand-$i.binlog")
    }
}

Invoke-MSBuild-Test "dotnet" "msbuild"
if ($IsWindows -and ($LASTEXITCODE -eq 0)) {Invoke-MSBuild-Test "msbuild" ""}

Compress-Archive $TestLogs -DestinationPath "test-logs.zip" -Force
exit $LASTEXITCODE
