$ErrorActionPreference = "Continue"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

$candidateRecoveryExePaths = @(
    (Join-Path $ScriptDir "..\outputs\OnlineClassGuardian-Admin\ParentRecovery\Recovery\Guardian.Recovery.exe"),
    (Join-Path $ScriptDir "..\outputs\OnlineClassGuardian\Recovery\Guardian.Recovery.exe")
)

foreach ($path in $candidateRecoveryExePaths) {
    $resolved = [System.IO.Path]::GetFullPath($path)
    if (Test-Path $resolved) {
        & $resolved
        exit $LASTEXITCODE
    }
}

Write-Host "[INFO] Published recovery tool not found. Running script-level recovery."
schtasks /Delete /TN OnlineClassGuardian /F | Out-Host
Get-Process Guardian.Daemon, Guardian.App -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "[OK] Attempted to remove the startup task and stop guardian processes. Logs are not deleted."
