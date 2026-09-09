# ============================================================================
# 02-populate-content.ps1
# For every  ## `<path>`  header in _dump.txt, writes the fenced code block
# that follows it into that file. Idempotent - safe to re-run.
#   -DryRun = print what would be written, write nothing.
# ============================================================================
param(
    [string]$DumpPath,
    [string]$RootPath,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

if (-not $DumpPath) {
    $DumpPath = Join-Path -Path $PSScriptRoot -ChildPath '_dump.txt'
}

if (-not $RootPath) {
    # Scripts live in  <repo>/frontend/src
    # Dump paths are relative to <repo>  (e.g. frontend/sentinelqa-web/package.json)
    # Use .NET API — avoids Split-Path -LiteralPath/-Parent parameter-set clash.
    $frontendDir = [System.IO.Path]::GetDirectoryName($PSScriptRoot)   # .../frontend
    $RootPath    = [System.IO.Path]::GetDirectoryName($frontendDir)    # .../SentinelQA (repo root)
}

if (-not (Test-Path -LiteralPath $DumpPath)) {
    Write-Error "Dump file not found: $DumpPath"
    exit 1
}

$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
$lines     = Get-Content -LiteralPath $DumpPath -Encoding UTF8

$currentFile = $null
$inFence     = $false
$fenceLen    = 0
$buffer      = [System.Collections.Generic.List[string]]::new()
$written     = [System.Collections.Generic.List[string]]::new()

foreach ($line in $lines) {
    if (-not $inFence) {
        # New file header:  ## `some/path.ext`
        if ($line -match '^##\s+`([^`]+)`\s*$') {
            $currentFile = $matches[1].Trim()
            continue
        }

        # Opening fence (``` or ````), only valid right after a header
        if ($currentFile -and ($line -match '^(`{3,})')) {
            $inFence  = $true
            $fenceLen = $matches[1].Length
            $buffer.Clear()
            continue
        }
    }
    else {
        # Closing fence: ONLY backticks, at least as long as the opener.
        # This lets the README's inner ```bash blocks survive inside ``` fences.
        if (($line -match '^(`{3,})\s*$') -and ($matches[1].Length -ge $fenceLen)) {
            $inFence = $false
            $full = Join-Path -Path $RootPath -ChildPath ($currentFile -replace '/', [IO.Path]::DirectorySeparatorChar)

            if (-not $DryRun) {
                $dir = [System.IO.Path]::GetDirectoryName($full)
                if ($dir -and -not (Test-Path -LiteralPath $dir)) {
                    New-Item -ItemType Directory -Path $dir -Force | Out-Null
                }
                [System.IO.File]::WriteAllText($full, ($buffer -join "`n"), $utf8NoBom)
            }
            $written.Add($currentFile)
            $currentFile = $null
            continue
        }
        $buffer.Add($line)
    }
}

# ---- Report ---------------------------------------------------------------
Write-Host ""
if ($DryRun) {
    Write-Host "DRY RUN - nothing written." -ForegroundColor Yellow
}
Write-Host "[OK] Files processed: $($written.Count)" -ForegroundColor Green

# Flag anything suspiciously small (likely a broken fence in the dump)
if (-not $DryRun) {
    $suspicious = foreach ($rel in $written) {
        $f = Join-Path -Path $RootPath -ChildPath ($rel -replace '/', [IO.Path]::DirectorySeparatorChar)
        if ((Test-Path -LiteralPath $f) -and ((Get-Item -LiteralPath $f).Length -lt 20)) {
            $rel
        }
    }
    if ($suspicious) {
        Write-Warning "These files are nearly empty - check their fences in _dump.txt:"
        $suspicious | ForEach-Object { Write-Warning "  $_" }
    }
}

Write-Host ""
Write-Host "First 15 written:" -ForegroundColor Cyan
$written | Select-Object -First 15 | ForEach-Object { Write-Host "  $_" }
if ($written.Count -gt 15) {
    Write-Host "  ... and $($written.Count - 15) more"
}