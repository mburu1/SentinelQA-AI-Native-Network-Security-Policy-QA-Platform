# ============================================================================
# 01-scaffold-structure.ps1
# Reads _dump.txt, finds every  ## `<path>`  header, and creates the
# folder structure + empty placeholder files.
# ============================================================================
param(
    [string]$DumpPath,
    [string]$RootPath,
    [switch]$ListOnly
)

$ErrorActionPreference = 'Stop'

# Resolve defaults in the body (keeps the param block simple & robust)
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

# ---- 1. Collect every target path from the dump ---------------------------
$paths = [System.Collections.Generic.List[string]]::new()
foreach ($line in (Get-Content -LiteralPath $DumpPath -Encoding UTF8)) {
    if ($line -match '^##\s+`([^`]+)`\s*$') {
        $paths.Add($matches[1].Trim())
    }
}

if ($paths.Count -eq 0) {
    Write-Error "No file entries found. Expected headers like:  ## ``frontend/sentinelqa-web/package.json``"
    exit 1
}

if ($ListOnly) {
    $paths | ForEach-Object { Write-Host $_ }
    Write-Host "Total: $($paths.Count)"
    exit 0
}

Write-Host "Dump     : $DumpPath" -ForegroundColor DarkGray
Write-Host "Repo root: $RootPath" -ForegroundColor DarkGray
Write-Host "Found $($paths.Count) files. Scaffolding..." -ForegroundColor Cyan

# ---- 2. Create folders + empty files -------------------------------------
$dirs = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($rel in $paths) {
    $full = Join-Path -Path $RootPath -ChildPath ($rel -replace '/', [IO.Path]::DirectorySeparatorChar)
    $dir  = [System.IO.Path]::GetDirectoryName($full)

    if ($dir -and -not (Test-Path -LiteralPath $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        [void]$dirs.Add($dir)
    }
    if (-not (Test-Path -LiteralPath $full)) {
        New-Item -ItemType File -Path $full -Force | Out-Null
    }
}

Write-Host ""
Write-Host "[OK] Scaffold complete." -ForegroundColor Green
Write-Host "   Directories : $($dirs.Count)"
Write-Host "   Files       : $($paths.Count)"
Write-Host ""
Write-Host "Next -> .\02-populate-content.ps1" -ForegroundColor Yellow