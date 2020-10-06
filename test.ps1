# This little script runs Sigourney's test project thrice to ensure it works repeatedly.
# It also keeps binary logs of each test run (which are kept as artifacts by AppVeyor).

function Test-ExitCode {if ($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}}

$TestLogs = './test-logs/'
$TestProject = './tests/test.proj'

if (Test-Path $TestLogs -PathType Container) {Remove-Item $TestLogs -Recurse -Force}

for ($i = 1; $i -le 3; $i++) {
    dotnet msbuild $TestProject ("/bl:{0}run-{1}.binlog" -f $TestLogs, $i)
    Test-ExitCode
}

Compress-Archive $TestLogs -DestinationPath "test-logs.zip" -Force
