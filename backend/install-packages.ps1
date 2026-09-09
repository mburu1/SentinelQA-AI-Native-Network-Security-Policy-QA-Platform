#!/usr/bin/env pwsh
<#
.SYNOPSIS
    SentinelQA backend — installs every NuGet dependency for all src + test projects.

.DESCRIPTION
    Run from the backend root:
        D:\Mwangi Wa Mburu\Coding\SentinelQA\backend

    - Test projects use xunit.v3 (legacy 'xunit' template packages are removed first).
    - Uses --no-restore per package for speed, then restores the solution once at the end.
    - With Central Package Management enabled, resolved versions are recorded
      automatically in Directory.Packages.props.

.EXAMPLE
    pwsh ./install-packages.ps1
#>

[CmdletBinding()]
param(
    [string]$BackendRoot = 'D:\Mwangi Wa Mburu\Coding\SentinelQA\backend'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

# Prefer the script's own folder if the solution lives next to it
if ($PSScriptRoot -and (Test-Path (Join-Path $PSScriptRoot 'SentinelQA.sln'))) {
    $BackendRoot = $PSScriptRoot
}

Set-Location $BackendRoot
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

Write-Host ''
Write-Host '============================================================' -ForegroundColor Green
Write-Host ' SentinelQA backend — NuGet installer (xunit.v3)'            -ForegroundColor Green
Write-Host " Root: $BackendRoot"                                        -ForegroundColor DarkGray
Write-Host '============================================================' -ForegroundColor Green

# Detect the solution file — modern .slnx preferred, legacy .sln as fallback
$SolutionFile = if (Test-Path 'SentinelQA.slnx') { 'SentinelQA.slnx' }
                elseif (Test-Path 'SentinelQA.sln') { 'SentinelQA.sln' }
                else { $null }

if (-not $SolutionFile) {
    throw "No SentinelQA.slnx or SentinelQA.sln found in '$BackendRoot'. Run this from the backend root."
}

Write-Host " Solution: $SolutionFile" -ForegroundColor DarkGray

# ---------------------------------------------------------------
# Central Package Management safety net
# ---------------------------------------------------------------
$cpmEnabled = (Test-Path 'Directory.Build.props') -and
              ((Get-Content 'Directory.Build.props' -Raw) -match 'ManagePackageVersionsCentrally\s*>\s*true')

if ($cpmEnabled -and -not (Test-Path 'Directory.Packages.props')) {
    Write-Host "`nCPM is enabled but Directory.Packages.props is missing — creating it." -ForegroundColor Yellow
    @'
<Project>
  <ItemGroup>
  </ItemGroup>
</Project>
'@ | Set-Content -Path 'Directory.Packages.props' -Encoding utf8
}

# ---------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------
function Add-Pkg {
    param(
        [Parameter(Mandatory)] [string] $Project,
        [Parameter(Mandatory)] [string] $Package
    )
    Write-Host ("  + {0}" -f $Package) -ForegroundColor Cyan
    & dotnet add $Project package $Package --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Failed: dotnet add $Project package $Package" }
}

function Remove-PkgIfExists {
    param([string]$Project, [string]$Package)
    $prev = $ErrorActionPreference
    $ErrorActionPreference = 'SilentlyContinue'
    & dotnet remove $Project package $Package *> $null
    $ErrorActionPreference = $prev
    if ($LASTEXITCODE -eq 0) { Write-Host ("  - removed legacy {0}" -f $Package) -ForegroundColor DarkGray }
}

function Add-TestFoundation {
    param([string]$Project)

    # Strip classic xunit template packages so xunit.v3 has zero conflicts
    foreach ($legacy in @('xunit','xunit.core','xunit.analyzers','xunit.assert','xunit.abstractions','xunit.extensibility.core','xunit.extensibility.execution')) {
        Remove-PkgIfExists $Project $legacy
    }

    Add-Pkg $Project 'xunit.v3'
    Add-Pkg $Project 'xunit.runner.visualstudio'
    Add-Pkg $Project 'Microsoft.NET.Test.Sdk'
    Add-Pkg $Project 'FluentAssertions'
    Add-Pkg $Project 'coverlet.collector'
}

function Ensure-LocalTool {
    param([string]$Name)
    $listed = (& dotnet tool list --local) -join "`n"
    if ($listed -notmatch [regex]::Escape($Name)) {
        Write-Host ("  + tool: {0}" -f $Name) -ForegroundColor Cyan
        & dotnet tool install $Name
        if ($LASTEXITCODE -ne 0) { throw "Failed installing local tool $Name" }
    }
}

# ---------------------------------------------------------------
# [1/9] Application layer
# ---------------------------------------------------------------
Write-Host "`n[1/9] SentinelQA.Application" -ForegroundColor Yellow
$app = 'src/SentinelQA.Application/SentinelQA.Application.csproj'
Add-Pkg $app 'MediatR'
Add-Pkg $app 'FluentValidation'

# Domain + Contracts are dependency-free by design — nothing to install.

# ---------------------------------------------------------------
# [2/9] Infrastructure
# ---------------------------------------------------------------
Write-Host "`n[2/9] SentinelQA.Infrastructure" -ForegroundColor Yellow
$infra = 'src/SentinelQA.Infrastructure/SentinelQA.Infrastructure.csproj'

# PostgreSQL / EF Core
Add-Pkg $infra 'Microsoft.EntityFrameworkCore'
Add-Pkg $infra 'Microsoft.EntityFrameworkCore.Relational'
Add-Pkg $infra 'Npgsql.EntityFrameworkCore.PostgreSQL'

# MongoDB / Redis
Add-Pkg $infra 'MongoDB.Driver'
Add-Pkg $infra 'StackExchange.Redis'

# Messaging — RabbitMQ (commands) + Kafka (events)
Add-Pkg $infra 'RabbitMQ.Client'
Add-Pkg $infra 'Confluent.Kafka'

# Background services (OutboxProcessor, consumers)
Add-Pkg $infra 'Microsoft.Extensions.Hosting.Abstractions'

# JWT issuing / hashing (Infrastructure.Security)
Add-Pkg $infra 'Microsoft.IdentityModel.Tokens'
Add-Pkg $infra 'System.IdentityModel.Tokens.Jwt'

# ---------------------------------------------------------------
# [3/9] Modules (bounded contexts)
# ---------------------------------------------------------------
$modules = @(
    'Identity','Tenants','Firewalls','Networks','Policies',
    'ChangeManagement','Testing','Defects','Notifications','Audit','Ai'
)

Write-Host "`n[3/9] Modules ($($modules.Count) bounded contexts)" -ForegroundColor Yellow
foreach ($m in $modules) {
    $proj = "src/Modules/SentinelQA.Modules.$m/SentinelQA.Modules.$m.csproj"
    Write-Host "  >> $m" -ForegroundColor DarkYellow
    Add-Pkg $proj 'MediatR'
    Add-Pkg $proj 'FluentValidation'
    Add-Pkg $proj 'FluentValidation.DependencyInjectionExtensions'
}

# ---------------------------------------------------------------
# [4/9] API host
# ---------------------------------------------------------------
Write-Host "`n[4/9] SentinelQA.Api" -ForegroundColor Yellow
$api = 'src/SentinelQA.Api/SentinelQA.Api.csproj'

# Auth / OpenAPI / docs
Add-Pkg $api 'Microsoft.AspNetCore.Authentication.JwtBearer'
Add-Pkg $api 'Microsoft.AspNetCore.OpenApi'
Add-Pkg $api 'Scalar.AspNetCore'

# Structured logging
Add-Pkg $api 'Serilog.AspNetCore'
Add-Pkg $api 'Serilog.Sinks.Console'

# Observability (OpenTelemetry)
Add-Pkg $api 'OpenTelemetry.Extensions.Hosting'
Add-Pkg $api 'OpenTelemetry.Instrumentation.AspNetCore'
Add-Pkg $api 'OpenTelemetry.Instrumentation.Http'
Add-Pkg $api 'OpenTelemetry.Instrumentation.Runtime'
Add-Pkg $api 'OpenTelemetry.Exporter.OpenTelemetryProtocol'
Add-Pkg $api 'Npgsql.OpenTelemetry'

# Health checks
Add-Pkg $api 'AspNetCore.HealthChecks.Npgsql'
Add-Pkg $api 'AspNetCore.HealthChecks.Redis'
Add-Pkg $api 'AspNetCore.HealthChecks.MongoDb'

# EF Core migrations tooling (design-time)
Add-Pkg $api 'Microsoft.EntityFrameworkCore.Design'

# ---------------------------------------------------------------
# [5/9] Unit tests
# ---------------------------------------------------------------
Write-Host "`n[5/9] SentinelQA.UnitTests" -ForegroundColor Yellow
$unit = 'tests/SentinelQA.UnitTests/SentinelQA.UnitTests.csproj'
Add-TestFoundation $unit
Add-Pkg $unit 'NSubstitute'

# ---------------------------------------------------------------
# [6/9] Integration tests (Testcontainers)
# ---------------------------------------------------------------
Write-Host "`n[6/9] SentinelQA.IntegrationTests" -ForegroundColor Yellow
$integration = 'tests/SentinelQA.IntegrationTests/SentinelQA.IntegrationTests.csproj'
Add-TestFoundation $integration
Add-Pkg $integration 'Testcontainers'
Add-Pkg $integration 'Testcontainers.PostgreSql'
Add-Pkg $integration 'Testcontainers.Redis'
Add-Pkg $integration 'Testcontainers.RabbitMq'
Add-Pkg $integration 'Testcontainers.Kafka'
Add-Pkg $integration 'WireMock.Net'

# ---------------------------------------------------------------
# [7/9] API + contract tests
# ---------------------------------------------------------------
Write-Host "`n[7/9] SentinelQA.ApiTests + SentinelQA.ContractTests" -ForegroundColor Yellow
$apiTests = 'tests/SentinelQA.ApiTests/SentinelQA.ApiTests.csproj'
Add-TestFoundation $apiTests
Add-Pkg $apiTests 'Microsoft.AspNetCore.Mvc.Testing'
Add-Pkg $apiTests 'Testcontainers.PostgreSql'

$contract = 'tests/SentinelQA.ContractTests/SentinelQA.ContractTests.csproj'
Add-TestFoundation $contract

# ---------------------------------------------------------------
# [8/9] E2E / performance / mutation tests
# ---------------------------------------------------------------
Write-Host "`n[8/9] E2E + Performance + Mutation tests" -ForegroundColor Yellow

$e2e = 'tests/SentinelQA.E2ETests/SentinelQA.E2ETests.csproj'
Add-TestFoundation $e2e
Add-Pkg $e2e 'Microsoft.Playwright'

$perf = 'tests/SentinelQA.PerformanceTests/SentinelQA.PerformanceTests.csproj'
Add-TestFoundation $perf
Add-Pkg $perf 'NBomber'

$mutation = 'tests/SentinelQA.MutationTests/SentinelQA.MutationTests.csproj'
if (Test-Path $mutation) {
    Add-TestFoundation $mutation   # Stryker itself runs as a dotnet tool (below)
}

# ---------------------------------------------------------------
# [9/9] Local .NET tools + final restore
# ---------------------------------------------------------------
Write-Host "`n[9/9] Local tools (dotnet-ef, dotnet-stryker) + restore" -ForegroundColor Yellow
if (-not (Test-Path '.config/dotnet-tools.json')) {
    & dotnet new tool-manifest *> $null
}
Ensure-LocalTool 'dotnet-ef'
Ensure-LocalTool 'dotnet-stryker'

Write-Host "`nRestoring solution..." -ForegroundColor Yellow
& dotnet restore 'SentinelQA.sln'
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

# ---------------------------------------------------------------
# Done
# ---------------------------------------------------------------
$stopwatch.Stop()
Write-Host ''
Write-Host '============================================================' -ForegroundColor Green
Write-Host (" Done in {0:mm\:ss}" -f $stopwatch.Elapsed)                      -ForegroundColor Green
Write-Host '============================================================' -ForegroundColor Green
Write-Host ' Next steps:'
Write-Host '   dotnet build SentinelQA.sln'
Write-Host '   dotnet test  SentinelQA.sln'
Write-Host '   dotnet ef migrations add InitialSchema -p src/SentinelQA.Infrastructure -s src/SentinelQA.Api'
Write-Host ''
Write-Host ' Note: Newman is an npm tool -> npm install -g newman' -ForegroundColor DarkGray
Write-Host ' Note: Playwright browsers  -> pwsh bin/Debug/net10.0/playwright.ps1 install (after first build)' -ForegroundColor DarkGray