param(
    [switch]$SelfTest,
    [switch]$Stress,
    [switch]$NoPlayer,
    [string]$PlayerPath = "$PSScriptRoot\Build\Windows\ShapeForge50.exe"
)

$ErrorActionPreference = "Stop"
$simRoot = "C:\Program Files\MetaXRSimulator\v207.1"
$simExe = Join-Path $simRoot "MetaXRSimulator.exe"
$activate = Join-Path $simRoot "activate_simulator.ps1"

Write-Host "ShapeForge 50 - Meta XR Simulator runner"

# Preflight: the simulator compositor needs a real GPU driver. On a software-only
# adapter it cannot create XR eye textures, so run the headless suites instead.
$gpus = @(Get-CimInstance Win32_VideoController | Select-Object -ExpandProperty Name)
$softwareOnly = ($gpus.Count -eq 0) -or (@($gpus | Where-Object { $_ -notmatch "Basic (Display|Render)|Remote Display|Hyper-V" }).Count -eq 0)
if ($softwareOnly) {
    Write-Warning ("No hardware GPU detected (" + ($gpus -join ", ") + "). Meta XR Simulator cannot composite XR here; " +
                   "running the headless self-test and stress test instead.")
    $headless = Join-Path $PSScriptRoot "Build\Windows\ShapeForge50.exe"
    if (!(Test-Path -LiteralPath $headless)) { throw "Build the desktop player first (ShapeForgeBuilder.BuildDesktopCI)." }
    foreach ($mode in @("-shapeforge-selftest", "-shapeforge-stress")) {
        $logName = ($mode -replace "-shapeforge-", "") + "_headless.log"
        $proc = Start-Process -FilePath $headless -PassThru -Wait -ArgumentList @("-batchmode", "-nographics", $mode, "-logFile", (Join-Path $PSScriptRoot $logName))
        Write-Host ("{0} exit code {1} (log: {2})" -f $mode, $proc.ExitCode, $logName)
        if ($proc.ExitCode -ne 0) { exit $proc.ExitCode }
    }
    exit 0
}

if (!(Test-Path -LiteralPath $simExe)) {
    throw "Meta XR Simulator not found at $simExe"
}

# Point the OpenXR runtime at the simulator (OVRPlugin routes through it).
$registry = "HKLM:\SOFTWARE\Khronos\OpenXR\1"
$current = (Get-ItemProperty -Path $registry -Name ActiveRuntime -ErrorAction SilentlyContinue).ActiveRuntime
if ($current -ne (Join-Path $simRoot "meta_openxr_simulator.json")) {
    Write-Host "Activating Meta XR Simulator as the OpenXR runtime..."
    & powershell -ExecutionPolicy Bypass -File $activate -MessageTime 1
}

# Windows App Runtime 1.5 is required by the simulator frontend.
$winApp = Join-Path $simRoot "WindowsAppRuntime\Microsoft.WindowsAppRuntime.1.5.msix"
if (Test-Path -LiteralPath $winApp) {
    $installed = Get-AppxPackage -Name Microsoft.WindowsAppRuntime.1.5 -ErrorAction SilentlyContinue
    if (-not $installed) {
        Write-Host "Installing Windows App Runtime 1.5..."
        & (Join-Path $simRoot "WindowsAppRuntime\install.bat")
    }
}

if (-not (Get-Process MetaXRSimulator -ErrorAction SilentlyContinue)) {
    Write-Host "Starting Meta XR Simulator frontend..."
    Start-Process -FilePath $simExe | Out-Null
    Start-Sleep -Seconds 8
} else {
    Write-Host "Meta XR Simulator frontend already running."
}

if ($NoPlayer) {
    Write-Host "Simulator started. Launch the ShapeForge scene in Unity or run this script without -NoPlayer."
    exit 0
}

if (!(Test-Path -LiteralPath $PlayerPath)) {
    Write-Host "Desktop player not found. Build it with:"
    Write-Host "  Unity.exe -batchmode -quit -projectPath <PROJECT> -executeMethod MetaXRSim.EditorTools.ShapeForgeBuilder.BuildDesktopCI"
    exit 2
}

$log = Join-Path $PSScriptRoot "simulator_player.log"
$playerArgs = @("-logFile", $log, "-screen-width", "1280", "-screen-height", "720")
if ($SelfTest) { $playerArgs += "-shapeforge-selftest" }
if ($Stress) { $playerArgs += "-shapeforge-stress" }

Write-Host "Launching $PlayerPath $($playerArgs -join ' ')"
$p = Start-Process -FilePath $PlayerPath -ArgumentList $playerArgs -PassThru
$p.WaitForExit()
Write-Host "Player exit code: $($p.ExitCode)"
Write-Host "Log: $log"

if ($SelfTest) {
    $report = Join-Path $env:USERPROFILE "AppData\LocalLow\ShapeForge Labs\ShapeForge 50\shapeforge_selftest.json"
    if (Test-Path -LiteralPath $report) {
        Copy-Item -LiteralPath $report -Destination (Join-Path $PSScriptRoot "Releases\shapeforge_selftest.json") -Force
        Write-Host "Self-test report copied to Releases\shapeforge_selftest.json"
    }
}

