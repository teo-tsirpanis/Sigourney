# This little script runs Sigourney's test project thrice to ensure it works repeatedly.
# It also keeps binary logs of each test run (which are kept as artifacts by AppVeyor).

function Test-ExitCode {if ($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}}
function DotnetClean {dotnet clean /v:m /nodereuse:false $TestProject; Test-ExitCode}
function Remove-Directory-Checked {
    param ([string]$Directory)
    if (Test-Path $Directory -PathType Container) {Remove-Item $Directory -Recurse -Force}
}

$TestLogs = './test-logs/'
$TestProject = './tests/test.proj'

Remove-Directory-Checked $TestLogs

DotnetClean
for ($i = 1; $i -le 3; $i++) {
    dotnet msbuild $TestProject /v:m /nodereuse:false ("/bl:{0}dotnet-{1}.binlog" -f $TestLogs, $i)
    Test-ExitCode
}

DotnetClean
for ($i = 1; $i -le 3; $i++) {
    msbuild $TestProject /v:m /nodereuse:false ("/bl:{0}msbuild-{1}.binlog" -f $TestLogs, $i)
    Test-ExitCode
}

Compress-Archive $TestLogs -DestinationPath "test-logs.zip" -Force
