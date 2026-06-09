param(
    [string]$DaemonPath = "",
    [string]$ConfigPath = ""
)

$ErrorActionPreference = "Stop"
$ServiceName = "OnlineClassGuardianService"

$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[FAILED] 请以管理员身份运行。"
    exit 1
}

if ([string]::IsNullOrWhiteSpace($DaemonPath)) {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $DaemonPath = Join-Path $ScriptDir "..\Daemon\Guardian.Daemon.exe"
    $DaemonPath = [System.IO.Path]::GetFullPath($DaemonPath)
}

if ([string]::IsNullOrWhiteSpace($ConfigPath)) {
    $ConfigPath = Join-Path $env:APPDATA "OnlineClassGuardian\config.json"
}

if (-not (Test-Path $DaemonPath)) {
    Write-Host "[FAILED] Guardian.Daemon.exe not found:"
    Write-Host $DaemonPath
    exit 1
}

Write-Host "[INFO] Service name: $ServiceName"
Write-Host "[INFO] Daemon path: $DaemonPath"
Write-Host "[INFO] Config path: $ConfigPath"

try {
    & $DaemonPath --install-service --start-service --config $ConfigPath
    Start-Sleep -Seconds 2
    sc.exe query $ServiceName | Out-Host
    Write-Host "[OK] 已请求安装并启动 Windows Service。"
    exit 0
} catch {
    Write-Host "[FAILED] Could not install/start service."
    Write-Host "[ERROR] $($_.Exception.Message)"
    exit 1
}
