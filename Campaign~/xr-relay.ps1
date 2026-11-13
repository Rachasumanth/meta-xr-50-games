# xr-relay.ps1 v4 - token-frugal campaign driver: relay reads state (cheap PS),
# LLM writes code (inline-context prompt, zero reads required)
$ErrorActionPreference = "Continue"
$proj = "C:\Users\Administrator\UnityProjects\MetaXRSim"
$camp = "$proj\Campaign~"
$oc   = "C:\Users\Administrator\AppData\Local\opencode\bin\opencode.exe"
$rlog = "$camp\relay.log"
$lastLaunch = Get-Date "1999-01-01"
$PACING = 6
$MODEL = if (Test-Path "$camp\model.txt") { (Get-Content "$camp\model.txt" -EA SilentlyContinue | Select-Object -First 1).Trim() } else { "google/gemini-3.5-flash-lite" }

function Log($m) { Add-Content -Path $rlog -Value ("[{0}] {1}" -f (Get-Date -Format s), $m) }
Log ("relay v4 starting model=" + $MODEL)

function Pick-Game {
  $cj = Get-Content "$camp\manifest.json" -Raw | ConvertFrom-Json
  $g = @($cj.games | Where-Object { $_.status -eq "gate_fail" }) | Select-Object -First 1
  if (-not $g) { $g = @($cj.games | Where-Object { $_.status -eq "building" }) | Select-Object -First 1 }
  if (-not $g) { $g = @($cj.games | Where-Object { $_.status -eq "pending" }) | Select-Object -First 1 }
  if ($g -and [int]$g.attempts -lt 6) { $g } else { $null }
}

function Set-Game([string]$slug, [string]$status, [string]$note, [switch]$IncrAttempts) {
  $man = "$camp\manifest.json"
  $cj = Get-Content $man -Raw | ConvertFrom-Json
  $g = $cj.games | Where-Object { $_.slug -eq $slug }
  if ($g) {
    $g.status = $status
    if ($note) { $g.note = $note }
    if ($IncrAttempts) { $g.attempts = [int]$g.attempts + 1 }
    $g.updated = (Get-Date -Format s)
    [IO.File]::WriteAllText($man, ($cj | ConvertTo-Json -Depth 6), (New-Object System.Text.UTF8Encoding($false)))
  }
  Log ("set " + $slug + " -> " + $status)
}

function Build-ManagerPrompt($g, [string]$failInfo = "") {
  $chapter = $g.chapter
  $slug = $g.slug
  $specPath = "$proj\Specs~\$chapter\$($g.file)"
  $folder = "$proj\Assets\Games\$chapter\$slug"

  $spec = ""
  if (Test-Path $specPath) { $spec = [IO.File]::ReadAllText($specPath) }
  $s15 = [regex]::Match($spec, "(?s)## 15\. Implementation Architecture.*?(?=## 16\.)").Value
  $s16 = [regex]::Match($spec, "(?s)## 16\. Acceptance Test Checklist.*").Value
  if (-not $s15) { $s15 = "## 15. Implementation Architecture (section marker not found in spec; infer classes)" }
  if (-not $s16) { $s16 = "## 16. Acceptance Test Checklist (section marker not found)" }

  $existing = "(none yet)"
  if (Test-Path "$folder\Scripts") {
    $parts = @()
    Get-ChildItem "$folder\Scripts" -Recurse -Filter *.cs -EA SilentlyContinue | ForEach-Object {
      $rel = $_.FullName.Replace("$folder\", "")
      $sigs = (Select-String -Path $_.FullName -Pattern "public\s+[\w<>\[\],\s]+\(" -EA SilentlyContinue | ForEach-Object { $_.Line.Trim() }) -join "`n"
      $props = (Select-String -Path $_.FullName -Pattern "public\s+[\w<>\[\]]+\s+\w+\s*\{\s*get" -EA SilentlyContinue | ForEach-Object { $_.Line.Trim() }) -join "`n"
      $parts += "[$rel]`n$sigs`n$props"
    }
    if ($parts.Count -gt 0) { $existing = $parts -join "`n`n" }
  }

  $ruleSet = @'
CAMPAIGN HARD RULES (violations fail the gate):
- Every class/struct/enum from section 15 defined EXACTLY ONCE across the game folder. Never duplicate types between files. CS0101 = automatic gate_fail.
- All spec constants go into ONE <Slug>Config static class (exact spec values with units).
- Game core classes are PURE C# (UnityEngine math types allowed). NO XR/OVR/hand-tracking/runtime dependencies in core files.
- ONE thin MonoBehaviour <Slug>Rig wires cores to transforms/Meta XR SDK at runtime.
- Ordinary law functions return RAW exact results (no pre-clamps). Caps are separate named functions/stages applied downstream on purpose.
- Quaternion integration uses Quaternion.AngleAxis(|omega|deg, axis); never Quaternion.Euler(components).
- Dead zones with release thresholds (spec 6) need stateful enter/release hysteresis.
- Tests: every mechanically-checkable item in section 16 becomes a [Test] method named Test<NN>_<ShortName> asserting the spec's exact numbers. Items needing device/rendering get [Category("NeedsDevice")] and are documented as deferred in BUILD_LOG.md. No Assert.IsTrue(true) on automated items.
- Editor tests live in Scripts/Editor/AcceptanceTests.cs, namespace XRGames.<ChapterPascal>.<SlugPascal>.Editor, using NUnit.Framework.
'@

  $prompt = @"
You are implementing ONE XR game as real production code in this Unity project.
GAME: $slug
CHAPTER: $chapter
GAME FOLDER: Assets/Games/$chapter/$slug/

DO NOT READ ANY FILES. Everything needed is inline below. Spend your tokens WRITING code, not reading.

EXISTING FILES AND THEIR PUBLIC MEMBERS (extend these classes if incomplete; never duplicate):
$existing

WRITE NOW (in as few batches as possible):
1. Scripts/<Slug>Config.cs if missing (constants; use existing if listed above).
2. Every section-15 class not already listed above: Scripts/<Class>.cs, real implementations per spec (no placeholders, no NotImplementedException).
3. Scripts/<Slug>Rig.cs thin MonoBehaviour.
4. Scripts/Editor/AcceptanceTests.cs - tests for ALL section-16 items per the TESTS rule below.
5. BUILD_LOG.md with: status, classes list, constants source, tests automated/deferred, honest limitations.

$ruleSet

--- SECTION 15 (implementation architecture) ---
$s15

--- SECTION 16 (acceptance test checklist) ---
$s16
END OF SECTIONS.
"@
  if ($failInfo) {
    $prompt += "`r`n--- PREVIOUS GATE FAILURES (fix exactly these inside the existing files; do not re-create everything) ---`r`n$failInfo`r`n"
  }
  $prompt += "Write all files now, or apply the minimal fix if FAILURES are listed, then end your turn. Do not queue games and do not touch manifest.json - the relay does that."
  return $prompt
}

function Queue-IfReady($g) {
  $folder = "$proj\Assets\Games\$($g.chapter)\$($g.slug)"
  $tests  = Test-Path "$folder\Scripts\Editor\AcceptanceTests.cs"
  $blog   = Test-Path "$folder\BUILD_LOG.md"
  $csCount = @(Get-ChildItem "$folder\Scripts" -Recurse -Filter *.cs -EA SilentlyContinue).Count
  if ($tests -and $blog -and $csCount -ge 3) {
    Set-Game $g.slug "gate_queued" "relay detected complete code set (tests+buildlog present); queued"
    [IO.File]::AppendAllText("$camp\gate-queue.txt", ("$($g.slug)" + [Environment]::NewLine))
    Log ("queued " + $g.slug + " (csFiles=" + $csCount + ")")
    return $true
  }
  return $false
}

function Get-GateFailInfo([string]$slug) {
  $info = @()
  $jr = "$camp\gate-results.jsonl"
  if (Test-Path $jr) {
    Get-Content $jr -EA SilentlyContinue | ForEach-Object {
      try {
        $o = $_ | ConvertFrom-Json
        if ($o.slug -eq $slug) {
          if ($o.lint) { $info += ("LINT: " + $o.lint) }
          if ($o.tests_failed_names) { $info += ("FAILED TESTS: " + $o.tests_failed_names) }
          if ($o.cs_errors -and [int]$o.cs_errors -gt 0) { $info += ("CS_ERRORS: " + $o.cs_errors) }
        }
      } catch {}
    }
  }
  return ($info -join "`n")
}

while ($true) {
  Start-Sleep 45
  # tunnel autorepair
  $l = Get-NetTCPConnection -State Listen -LocalPort 30000 -EA SilentlyContinue
  if (-not $l) { Start-ScheduledTask -TaskName "tunnel-003-http" -EA SilentlyContinue; Start-Sleep 8;
    $l = Get-NetTCPConnection -State Listen -LocalPort 30000 -EA SilentlyContinue
    if ($l) { Log "tunnel 30000 auto-restarted" } }

  # Unity gate when queue non-empty and Unity idle
  $q = "$camp\gate-queue.txt"
  if (Test-Path $q) {
    $n = @(Get-Content $q -EA SilentlyContinue | Where-Object { "$_".Trim() -ne "" })
    $unityRunning = Get-Process Unity -EA SilentlyContinue
    if ($n.Count -gt 0 -and -not $unityRunning) {
      Log ("gate triggered for " + ($n -join ","))
      & powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$camp\xr-gate.ps1"
      Log "gate returned"
    }
  }

  # manager lifecycle with token-frugal inline prompt
  $ops = @(Get-Process opencode -EA SilentlyContinue)
  $age = ((Get-Date) - $lastLaunch).TotalMinutes
  if ($ops.Count -eq 0 -and $age -ge $PACING) {
    $g = Pick-Game
    if ($g) {
      $env:HTTPS_PROXY = "http://127.0.0.1:30000"
      $env:HTTP_PROXY  = "http://127.0.0.1:30000"
      $env:NO_PROXY    = "localhost,127.0.0.1,::1"
      $ts = Get-Date -Format "yyyyMMdd_HHmmss"
      if ($g.status -eq "building") {
        # continue/fix run only if the code set is not ready for the gate yet
        if (Queue-IfReady $g) {
          $lastLaunch = (Get-Date).AddMinutes(-$PACING)  # do not block the gate cycle
          Log "building game was ready - queued for gate without a manager run"
          continue
        }
        Set-Game $g.slug "building" "continue run on partially built game" -IncrAttempts
      } elseif ($g.status -eq "gate_fail") {
        Set-Game $g.slug "building" "fix run after gate_fail" -IncrAttempts
      } else {
        Set-Game $g.slug "building" "relay picked game; worker writing code" -IncrAttempts
      }
      $failInfo = Get-GateFailInfo $g.slug
      $prompt = Build-ManagerPrompt $g $failInfo
      # one quoted argv token: flatten newlines + strip quotes so the whole
      # prompt arrives as the message, not as flags
      $promptClean = (($prompt -replace '"', '') -replace "`r", " ") -replace "`n", " "
      $argLine = 'run --model ' + $MODEL + ' "' + $promptClean + '"'
      Start-Process -FilePath $oc -ArgumentList $argLine -WorkingDirectory $proj -RedirectStandardOutput "$camp\manager-$ts.out" -RedirectStandardError "$camp\manager-$ts.err" -WindowStyle Hidden
      $lastLaunch = Get-Date
      Log ("manager launched serial [" + $g.slug + "] (prev=" + $g.status + " haveFailInfo=" + [bool]$failInfo + ")")
    } else {
      Log "no games to work (all complete or attempts exhausted) - idling"
      $lastLaunch = Get-Date
    }
  }
  if ($ops.Count -gt 0) {
    $mgr = Get-ChildItem "$camp\manager-*.out" -EA SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($mgr -and ((Get-Date) - $mgr.LastWriteTime).TotalMinutes -gt 40) {
      Log "watchdog: manager silent 40 min, kill"
      $ops | Stop-Process -Force -EA SilentlyContinue
      $lastLaunch = (Get-Date).AddMinutes(-$PACING)
    }
  }
}
