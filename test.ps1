# This little script runs Sigourney's test project thrice to ensure it works repeatedly.
# It also keeps binary logs of each test run (which are kept as artifacts by AppVeyor).

function Test-ExitCode {if ($LASTEXITCODE -ne 0) {exit $LASTEXITCODE}}

$TestProject = './tests/test.proj'

for ($i = 1; $i -le 3; $i++) {
    dotnet msbuild $TestProject /bl:run-$i.binlog
    Test-ExitCode
}
