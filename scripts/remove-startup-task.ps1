$TaskName = "OnlineClassGuardian"

schtasks /Delete /TN $TaskName /F | Out-Host
if ($LASTEXITCODE -eq 0) {
    Write-Host "[完成] 已删除计划任务：$TaskName"
} else {
    Write-Host "[提示] 删除计划任务未成功，可能任务不存在。"
}
