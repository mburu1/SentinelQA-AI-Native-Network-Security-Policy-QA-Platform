#Requires -Version 5.1
<#
.SYNOPSIS
    Parses _dump.txt and materializes the complete SentinelQA backend source tree.
#>

$ErrorActionPreference = 'Stop'
$Base     = 'D:\Mwangi Wa Mburu\Coding\SentinelQA\backend'
$DumpFile = Join-Path $Base '_dump.txt'

if (-not (Test-Path $DumpFile)) {
    Write-Error "Dump file not found: $DumpFile"
    exit 1
}

Write-Host "`nReading dump..." -ForegroundColor Cyan
$dump = [System.IO.File]::ReadAllText($DumpFile, [System.Text.Encoding]::UTF8)

# Robust pattern:
# - tolerates optional whitespace
# - optional language tag after ```
# - works with both LF and CRLF
$pattern = '(?ms)^\*\*`(?<path>[^`]+)`\*\*\s*```(?:[a-zA-Z0-9_+-]*)?\s*\r?\n(?<code>.*?)\r?\n```'

$matches = [regex]::Matches($dump, $pattern)

if ($matches.Count -eq 0) {
    Write-Warning "No file blocks matched. Check the format of _dump.txt."
    exit 1
}

Write-Host "Found $($matches.Count) files. Writing..." -ForegroundColor Cyan
Write-Host ""

$written = 0
$skipped = 0

foreach ($m in $matches) {
    $rel  = $m.Groups['path'].Value.Trim()
    $code = $m.Groups['code'].Value

    # Skip empty / non-code blocks
    if ([string]::IsNullOrWhiteSpace($code) -or $rel -notmatch '\.(cs|json|yml|yaml|ps1|md)$') {
        $skipped++
        continue
    }

    $full = Join-Path $Base $rel
    $dir  = Split-Path $full -Parent

    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }

    # Write as UTF-8 without BOM, preserve original newlines
    [System.IO.File]::WriteAllText($full, $code, [System.Text.UTF8Encoding]::new($false))
    Write-Host "  + $rel" -ForegroundColor Green
    $written++
}

Write-Host ""
Write-Host "Finished." -ForegroundColor Cyan
Write-Host "  Written : $written" -ForegroundColor Green
Write-Host "  Skipped : $skipped" -ForegroundColor Yellow
Write-Host ""
Write-Host "You can now delete _dump.txt if you want." -ForegroundColor DarkGray