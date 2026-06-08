param(
    [string]$DaemonPath = ""
)

$ErrorActionPreference = "Stop"
$TaskName = "OnlineClassGuardian"

$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "[FAILED] Please run this script as Administrator."
    exit 1
}

if ([string]::IsNullOrWhiteSpace($DaemonPath)) {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $DaemonPath = Join-Path $ScriptDir "..\Daemon\Guardian.Daemon.exe"
    $DaemonPath = [System.IO.Path]::GetFullPath($DaemonPath)
}

if (-not (Test-Path $DaemonPath)) {
    Write-Host "[FAILED] Guardian.Daemon.exe not found:"
    Write-Host $DaemonPath
    exit 1
}

$CurrentUser = [Security.Principal.WindowsIdentity]::GetCurrent().Name
$WorkingDir = Split-Path -Parent $DaemonPath

Write-Host "[INFO] Task name: $TaskName"
Write-Host "[INFO] User: $CurrentUser"
Write-Host "[INFO] Daemon path: $DaemonPath"

try {
    $Action = New-ScheduledTaskAction -Execute $DaemonPath -WorkingDirectory $WorkingDir
    $Trigger = New-ScheduledTaskTrigger -AtLogOn -User $CurrentUser
    $Principal = New-ScheduledTaskPrincipal -UserId $CurrentUser -LogonType Interactive -RunLevel Highest
    $Settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -MultipleInstances IgnoreNew -ExecutionTimeLimit (New-TimeSpan -Seconds 0)

    Register-ScheduledTask -TaskName $TaskName -Action $Action -Trigger $Trigger -Principal $Principal -Settings $Settings -Force | Out-Null

    Write-Host "[OK] Created elevated startup task: $TaskName"
    Write-Host "[OK] Daemon path: $DaemonPath"
    exit 0
} catch {
    Write-Host "[FAILED] Could not create startup task."
    Write-Host "[ERROR] $($_.Exception.Message)"
    if ($_.Exception.InnerException) {
        Write-Host "[ERROR-INNER] $($_.Exception.InnerException.Message)"
    }
    exit 1
}
