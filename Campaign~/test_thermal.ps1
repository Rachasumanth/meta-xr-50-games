$T = 24.0
$dt = 0.001
for ($t = 0; $t -lt 15.0; $t += $dt) {
    $dT = (28.0 - 0.12 * ($T - 24.0)) / 1.2 * $dt
    $T += $dT
    if ([Math]::Abs($T - 150.0) -lt 0.05) { Write-Host "T=150 at t=$t" }
    if ([Math]::Abs($T - 180.0) -lt 0.05) { Write-Host "T=180 at t=$t" }
}
Write-Host "Final T at 15s = $T"
