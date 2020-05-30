$rootPath = $global:PSScriptRoot
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$rootPath\pChat.lnk")
$Shortcut.TargetPath = "$PsHome\powershell.exe"
$Shortcut.Arguments = "-noexit -ExecutionPolicy Bypass -File $rootPath\Launch_pChat.ps1"
$Shortcut.IconLocation = "$rootPath\resources\pChatIcon.ico"
$Shortcut.Save()