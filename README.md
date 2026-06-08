# Online Class Guardian for Windows

Online Class Guardian is a local Windows tool for parents who need a focused online-class mode. It was built for scenarios where a child attends class in Tencent Meeting and should still be able to open local course files, while browsers, games, emulators, app stores, downloaders, social apps, and music apps are closed during class time.

## What It Does

- Manual enable/disable of online-class mode.
- Parent password required to disable locked mode.
- Lock state persists after restart.
- Elevated scheduled task support for stronger process termination.
- Allows Tencent Meeting and common local document/image tools.
- Blocks common distraction software by process name, path keyword, and window title.
- Keeps network proxy tools such as v2rayN, Clash, and Shadowsocks out of the block list.
- Shows randomized encouraging reminders when a blocked app is closed.
- Stores configuration and logs locally only.

## Projects

- `Guardian.Shared`: configuration, password hashing, default rules, matching, logging, and reminder selection.
- `Guardian.Daemon`: background monitor that closes blocked processes.
- `Guardian.App`: WPF tray/control app for parents.
- `Guardian.Recovery`: parent recovery utility.
- `Guardian.Shared.Tests`: unit tests for core behavior.

## Requirements

- Windows 10/11
- .NET 8 SDK for development
- .NET 8 Desktop Runtime for running framework-dependent builds

## Build And Test

```powershell
dotnet test .\src\OnlineClassGuardian.sln
dotnet publish .\src\Guardian.App\Guardian.App.csproj -c Release -r win-x64 --self-contained false -o .\outputs\OnlineClassGuardian-Admin\App
dotnet publish .\src\Guardian.Daemon\Guardian.Daemon.csproj -c Release -r win-x64 --self-contained false -o .\outputs\OnlineClassGuardian-Admin\Daemon
dotnet publish .\src\Guardian.Recovery\Guardian.Recovery.csproj -c Release -r win-x64 --self-contained false -o .\outputs\OnlineClassGuardian-Admin\ParentRecovery\Recovery
```

After publishing, run `scripts\install-startup-task.ps1` from an elevated PowerShell, passing the daemon path if needed.

## Privacy And Safety

This repository does not include local runtime configuration, logs, passwords, password hashes from a real machine, or published binaries. The app stores its runtime data under the current user's application data directory.

This is a focus-assist tool, not tamper-proof security software. A technically skilled local administrator can still bypass or remove it.
