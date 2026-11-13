$proj = 'C:\Users\Administrator\UnityProjects\MetaXRSim'
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.64f1\Editor\Unity.exe'
$log = "$proj\ux_simulation.log"

if (Test-Path $log) { Remove-Item $log -Force }

Write-Host "Launching Unity UXSimulator..."
$p = Start-Process -FilePath $unity -ArgumentList "-batchmode","-quit","-projectPath",$proj,"-executeMethod","XRGames.UXTesting.UXSimulator.RunAll","-logFile",$log -PassThru -WindowStyle Hidden

$sw = [Diagnostics.Stopwatch]::StartNew()
while (-not $p.HasExited -and $sw.Elapsed.TotalMinutes -lt 4) {
    Start-Sleep -Seconds 5
    Write-Host ("Waiting... " + [math]::Round($sw.Elapsed.TotalSeconds, 1) + "s (PID: " + $p.Id + ")")
}

if (-not $p.HasExited) {
    Stop-Process -Id $p.Id -Force
    Write-Host "Unity timed out and was killed."
} else {
    Write-Host ("Unity exited with code: " + $p.ExitCode)
}
