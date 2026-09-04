#!/usr/bin/env pwsh
Write-Host "Starting SentinelQA infrastructure..." -ForegroundColor Cyan
docker compose up -d
Write-Host "Verifying containers..." -ForegroundColor Cyan
docker compose ps
Write-Host "Done. API: dotnet run --project backend/src/SentinelQA.Api" -ForegroundColor Green