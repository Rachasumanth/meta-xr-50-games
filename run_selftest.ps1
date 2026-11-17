param(
    [string]$PlayerPath = "$PSScriptRoot\Build\Windows\ShapeForge50.exe"
)

$ErrorActionPreference = "Stop"
if (!(Test-Path -LiteralPath $PlayerPath)) {
    throw "Desktop player not found at $PlayerPath. Build it first with MetaXRSim.EditorTools.ShapeForgeBuilder.BuildDesktopCI."
}

$log = Join-Path $PSScriptRoot "selftest_player.log"
Write-Host "Running ShapeForge 50 headless self-test..."
& $PlayerPath -batchmode -nographics -shapeforge-selftest -logFile $log
$code = $LASTEXITCODE
Write-Host "Exit code: $code"
Write-Host "Log: $log"

$report = Join-Path $env:USERPROFILE "AppData\LocalLow\ShapeForge Labs\ShapeForge 50\shapeforge_selftest.json"
if (Test-Path -LiteralPath $report) {
    New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot "Releases") -Force | Out-Null
    Copy-Item -LiteralPath $report -Destination (Join-Path $PSScriptRoot "Releases\shapeforge_selftest.json") -Force
    Write-Host "Report copied to Releases\shapeforge_selftest.json"
}

exit $code
