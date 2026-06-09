$ServiceName = "OnlineClassGuardianService"
$TaskName = "OnlineClassGuardian"

& sc.exe stop $ServiceName | Out-Host
& sc.exe delete $ServiceName | Out-Host
& schtasks /Delete /TN $TaskName /F | Out-Host

Write-Host "[OK] Attempted to remove Windows Service and legacy scheduled task."
