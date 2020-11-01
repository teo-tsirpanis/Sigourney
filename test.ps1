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

DotnetClean
for ($i = 1; ($i -le 3) -and ($LASTEXITCODE -eq 0); $i++) {
    dotnet msbuild $TestProject /v:m /nodereuse:false ("/bl:{0}dotnet-{1}.binlog" -f $TestLogs, $i)
}

if ($IsWindows -and ($LASTEXITCODE -eq 0)) {
    DotnetClean
    for ($i = 1; ($i -le 3) -and ($LASTEXITCODE -eq 0); $i++) {
        msbuild $TestProject /v:m /nodereuse:false ("/bl:{0}msbuild-{1}.binlog" -f $TestLogs, $i)
    }
}

Compress-Archive $TestLogs -DestinationPath "test-logs.zip" -Force
exit $LASTEXITCODE
