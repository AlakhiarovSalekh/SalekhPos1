# SalekhPos — local toolchain setup (Windows / PowerShell)
#
# This script documents and automates the install of the tools required
# to develop SalekhPos locally. The .NET 8 SDK, Node.js, and Git are the
# only required pieces; everything else (PostgreSQL, Redis, the Caddy
# reverse proxy) is provided through Docker, which is installed by this
# script too.
#
# Run from an elevated PowerShell prompt:
#   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
#   .\scripts\setup.ps1
#
# What this script does:
#   1. Installs the .NET 8 SDK if not present (via winget).
#   2. Installs Node.js 20 LTS if not present (via winget).
#   3. Installs Docker Desktop if not present (via winget).
#   4. Installs GitHub CLI if not present (via winget).
#   5. Prints a verification report.
#
# What this script does NOT do (run them manually after):
#   - Create the SalekhPos GitHub repository (you did this).
#   - Sign into GitHub via `gh auth login`.
#   - Enable WSL2 + Hyper-V for Docker (Docker Desktop installer does
#     this, but a reboot is required).
#   - Push the local repo to GitHub.

[CmdletBinding()]
param(
    [switch]$SkipDotnet,
    [switch]$SkipNode,
    [switch]$SkipDocker,
    [switch]$SkipGh
)

$ErrorActionPreference = 'Stop'

function Test-Command {
    param([string]$Command)
    $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Install-WithWinget {
    param(
        [Parameter(Mandatory)] [string] $Id,
        [Parameter(Mandatory)] [string] $FriendlyName
    )
    Write-Host "Installing $FriendlyName ($Id) via winget..." -ForegroundColor Cyan
    winget install --id $Id --accept-package-agreements --accept-source-agreements --silent
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "winget install of $Id exited with $LASTEXITCODE. Please install $FriendlyName manually."
    }
}

Write-Host "== SalekhPos toolchain setup ==" -ForegroundColor Green

# 1. .NET 8 SDK
if (-not $SkipDotnet) {
    if (Test-Command 'dotnet') {
        $existing = (& dotnet --list-sdks) -join ', '
        if ($existing -match '8\.') {
            Write-Host "dotnet SDK already installed: $existing" -ForegroundColor Green
        } else {
            Write-Host "dotnet found but no 8.x SDK installed. Installing Microsoft.DotNet.SDK.8..." -ForegroundColor Yellow
            Install-WithWinget -Id 'Microsoft.DotNet.SDK.8' -FriendlyName '.NET 8 SDK'
        }
    } else {
        Install-WithWinget -Id 'Microsoft.DotNet.SDK.8' -FriendlyName '.NET 8 SDK'
    }
}

# 2. Node.js 20 LTS
if (-not $SkipNode) {
    if (Test-Command 'node') {
        $nodeVersion = & node --version
        Write-Host "node already installed: $nodeVersion" -ForegroundColor Green
    } else {
        Install-WithWinget -Id 'OpenJS.NodeJS.LTS' -FriendlyName 'Node.js 20 LTS'
    }
}

# 3. Docker Desktop
if (-not $SkipDocker) {
    if (Test-Command 'docker') {
        $dockerVersion = & docker --version
        Write-Host "docker already installed: $dockerVersion" -ForegroundColor Green
    } else {
        Install-WithWinget -Id 'Docker.DockerDesktop' -FriendlyName 'Docker Desktop'
        Write-Host "Docker Desktop installer started. A reboot may be required to enable WSL2/Hyper-V." -ForegroundColor Yellow
    }
}

# 4. GitHub CLI
if (-not $SkipGh) {
    if (Test-Command 'gh') {
        $ghVersion = & gh --version | Select-Object -First 1
        Write-Host "gh already installed: $ghVersion" -ForegroundColor Green
    } else {
        Install-WithWinget -Id 'GitHub.cli' -FriendlyName 'GitHub CLI'
    }
}

Write-Host ""
Write-Host "== Verification ==" -ForegroundColor Green
if (Test-Command 'dotnet') { & dotnet --list-sdks } else { Write-Host "dotnet: MISSING" }
if (Test-Command 'node')   { & node --version }     else { Write-Host "node: MISSING" }
if (Test-Command 'npm')    { & npm --version }      else { Write-Host "npm: MISSING" }
if (Test-Command 'docker') { & docker --version }   else { Write-Host "docker: MISSING" }
if (Test-Command 'git')    { & git --version }      else { Write-Host "git: MISSING" }
if (Test-Command 'gh')     { & gh --version | Select-Object -First 1 } else { Write-Host "gh: MISSING" }

Write-Host ""
Write-Host "== Next manual steps ==" -ForegroundColor Green
Write-Host "1. Sign into GitHub: gh auth login"
Write-Host "2. Confirm the SalekhPos repo remote: git -C \$PWD remote -v"
Write-Host "3. (If needed) update the placeholder remote URL:"
Write-Host "     git remote set-url salekhpos https://github.com/AlakhiarovSalekh/SalekhPos.git"
Write-Host "4. Push: git push -u salekhpos main"
Write-Host "5. Verify the build:"
Write-Host "     cd backend; dotnet build; dotnet test"
Write-Host "     cd ../web/salekhpos-web; npm install; npm run build; npm test"
Write-Host "6. Bring up the dev stack once Docker is running:"
Write-Host "     docker compose -f infrastructure/docker/docker-compose.yml up"
