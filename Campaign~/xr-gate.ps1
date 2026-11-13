param([switch]$Once)
$ErrorActionPreference = "Continue"
$proj  = "C:\Users\Administrator\UnityProjects\MetaXRSim"
$camp  = "$proj\Campaign~"
$unity = "C:\Program Files\Unity\Hub\Editor\6000.0.64f1\Editor\Unity.exe"
$queue = "$camp\gate-queue.txt"
$res   = "$camp\results"
$man   = "$camp\manifest.json"
$glog  = "$camp\gate.log"

function Log($m) { Add-Content -Path $glog -Value ("[{0}] {1}" -f (Get-Date -Format s), $m) }

function Update-Manifest([string]$slug, [hashtable]$fields) {
  $m = Get-Content $man -Raw -Encoding UTF8 | ConvertFrom-Json
  foreach ($g in $m.games) {
    if ($g.slug -eq $slug -or ($g.slug -replace '-', '_') -eq ($slug -replace '-', '_')) {
      foreach ($k in $fields.Keys) { $g.$k = $fields[$k] }
      break
    }
  }
  $m | ConvertTo-Json -Depth 6 | Set-Content $man -Encoding UTF8
}

# ---- queue ----
if (-not (Test-Path $queue)) { exit 0 }
$slugs = @(Get-Content $queue | Where-Object { "$_".Trim() -ne "" } | ForEach-Object { "$_".Trim() })
if ($slugs.Count -eq 0) { exit 0 }
# claim the queue (clear it)
Set-Content -Path $queue -Value "" -Encoding ASCII
Log ("gate start slugs=" + ($slugs -join ","))

New-Item -ItemType Directory -Force -Path $res | Out-Null
$ts = Get-Date -Format "yyyyMMdd_HHmmss"
$xml  = "$res\run_$ts.xml"
$ulog = "$res\run_$ts.log"

$budget = 40
if (-not (Test-Path "$proj\Library\PackageCache\com.unity.test-framework")) { $budget = 60 }
$p = Start-Process -FilePath $unity -ArgumentList "-batchmode","-nographics","-projectPath",$proj,"-runTests","-testPlatform","EditMode","-testResults",$xml,"-logFile",$ulog -PassThru -WindowStyle Hidden
$sw = [Diagnostics.Stopwatch]::StartNew()
while (-not $p.HasExited -and $sw.Elapsed.TotalMinutes -lt $budget) { Start-Sleep 10 }
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue; Log "UNITY TIMEOUT" }
$exit = $p.ExitCode
Log ("unity exit=" + $exit)

# ---- static anti-cheat lint per game ----
$lintNote = @{}
foreach ($s in $slugs) {
  $slug  = [string]$s
  $folder = Get-ChildItem "$proj\Assets\Games" -Recurse -Directory -Filter $slug -EA SilentlyContinue | Select-Object -First 1
  if (-not $folder) { $lintNote[$slug] = "no folder"; continue }
  $cs = @(Get-ChildItem "$($folder.FullName)\Scripts" -Recurse -Filter *.cs -EA SilentlyContinue)
  if ($cs.Count -eq 0) { $lintNote[$slug] = "no .cs files"; continue }
  $paths = ($cs | ForEach-Object { $_.FullName })
  if (@(Select-String -Path $paths -Pattern "NotImplementedException" -EA SilentlyContinue).Count -gt 0) { $lintNote[$slug] = "NotImplementedException present"; continue }
  $tests = @($cs | Where-Object { "$($_.FullName)" -match "AcceptanceTests" })
  if ($tests.Count -eq 0) { $lintNote[$slug] = "no AcceptanceTests.cs under Scripts"; continue }
  if (@(Select-String -Path ($tests | ForEach-Object { $_.FullName }) -Pattern "\[Test\]" -EA SilentlyContinue).Count -eq 0) { $lintNote[$slug] = "AcceptanceTests.cs has no [Test] bodies" }
}

# ---- compile-error attribution ----
$errByGame = @{}
Select-String -Path $ulog -Pattern "error CS\d+" -EA SilentlyContinue | ForEach-Object {
  $line = $_.Line
  foreach ($slug in $slugs) {
    if ($line -match [regex]::Escape($slug)) {
      $errByGame[$slug] = ($errByGame[$slug] + 1); break
    }
  }
}

# ---- NUnit XML parse ----
$cases = @()
if (Test-Path $xml) {
  try {
    [xml]$rx = Get-Content $xml -Raw -Encoding UTF8
    $cases = $rx.SelectNodes("//*[local-name()='test-case']") | ForEach-Object { [pscustomobject]@{ name = $_.fullname; res = $_.result } }
  } catch { Log ("xml parse fail: " + $_.Exception.Message) }
} else { Log "results xml missing" }

# ---- per-game verdicts (gate is the ONLY writer of these statuses) ----
foreach ($slug in $slugs) {
  $pascal = (($slug -split "_") | Where-Object { $_.Length -gt 0 } | ForEach-Object { $_.Substring(0,1).ToUpper() + $_.Substring(1) }) -join ""
  $pat = "(?i)[\.\-](" + [regex]::Escape($slug) + "|" + [regex]::Escape($pascal) + ")"
  $mine = @($cases | Where-Object { $_.name -match $pat })
  $total = $mine.Count
  $failed = @($mine | Where-Object { $_.res -match "(?i)fail|error|inconclusive" }).Count
  $needDev = @($mine | Where-Object { $_.res -match "^(skipped|inconclusive)" }).Count
  $csE = 0; if ($errByGame.ContainsKey($slug)) { $csE = $errByGame[$slug] }
  $lint = $lintNote[$slug]
  $passCount = @($mine | Where-Object { "$($_.res)" -match "(?i)passed" }).Count
  $verdict = ""
  $status  = "gate_fail"
  if ($lint) { $verdict = "LINT: $lint" }
  elseif (-not (Test-Path $xml)) { $verdict = "no results xml (unityExit=$exit)" }
  elseif ($total -eq 0) { $verdict = "no tests registered for this game" }
  elseif ($csE -gt 0) { $verdict = "compile failed (CSerrs=$csE)" }
  elseif ($failed -gt 0) { $verdict = "tests failed=$failed of $total passed=$passCount" }
  elseif ($exit -eq 0) { $status = "complete"; $verdict = "PASS tests=$total passed=$passCount" }
  else { $verdict = "unity exit nonzero with xml clean (exit=$exit), investigate" }
  $note = "run_$ts | $verdict"
  Log ("slug=" + $slug + " verdict=" + $verdict + " status=" + $status)
  Update-Manifest $slug @{ status = $status; note = $note; updated = (Get-Date -Format s) } 
}

# ---- machine-readable results for the manager ----
$jr = "$camp\gate-results.jsonl"
foreach ($slug in $slugs) {
  $pascal = (($slug -split "_") | Where-Object { $_.Length -gt 0 } | ForEach-Object { $_.Substring(0,1).ToUpper() + $_.Substring(1) }) -join ""
  $pat = "(?i)[\.\-](" + [regex]::Escape($slug) + "|" + [regex]::Escape($pascal) + ")"
  $mine = @($cases | Where-Object { $_.name -match $pat })
  $failNames = (@($mine | Where-Object { $_.res -match "(?i)fail|error|inconclusive" }).name) -join " ; "
  $line = @{ ts = (Get-Date -Format s); run = "run_$ts"; slug = $slug;
             tests_total = $mine.Count;
             tests_passed = $(@($mine | Where-Object { "$($_.res)" -match "(?i)passed" }).Count);
             tests_failed_names = $failNames;
             cs_errors = $(if ($errByGame.ContainsKey($slug)) { $errByGame[$slug] } else { 0 }); lint = $lintNote[$slug];
             unity_exit = $exit } | ConvertTo-Json -Compress
  Add-Content -Path $jr -Value $line -Encoding UTF8
}
Log "gate done"
