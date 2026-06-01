# Run this whenever VS reports "NuGet package restore failed" or NETSDK1005.
# VS's auto-restore uses a different NuGet client and breaks the assets files.
# This uses VS's own MSBuild (same client, matching hash) so VS treats the result as current.

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" 2>$null

if (-not $msbuild) {
    Write-Error "Could not find MSBuild via vswhere."
    exit 1
}

$sln = Get-Item "$PSScriptRoot\*.sln" | Select-Object -First 1
if (-not $sln) {
    Write-Error "No .sln file found in $PSScriptRoot"
    exit 1
}

Write-Host "Restoring $($sln.Name) using $msbuild ..." -ForegroundColor Cyan
& $msbuild $sln.FullName /t:Restore /p:Configuration=Debug /v:minimal /nologo

if ($LASTEXITCODE -eq 0) {
    Write-Host "Restore succeeded." -ForegroundColor Green
} else {
    Write-Error "Restore failed (exit code $LASTEXITCODE)."
    exit $LASTEXITCODE
}
