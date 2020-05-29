# Generate a random Alphanumeric string

Function Get-RandomAlphanumericString {
	
	[CmdletBinding()]
	Param (
        [int] $length = 6
	)

	Begin{
	}

	Process{
        Write-Output ( -join ((0x30..0x39) + ( 0x41..0x5A) + ( 0x61..0x7A) | Get-Random -Count $length  | % {[char]$_}) )
	}	
}

#dir | where {$_.PsIsContainer} | Select-Object *.txt
#dir . -filter "*.txt"

$Chatroom = Read-Host -Prompt 'Chat room to join/create (leave blank for a generated name)'

If ($Chatroom -eq "") {
    $Chatroom = (Get-RandomAlphanumericString | Tee-Object -variable teeTime).ToLower()
    Write-Host "Room Name: $Chatroom"
}

Import-Module .\chat.ps1
Enter-Chat -ChatChannelName $Chatroom -Name $env:USERNAME -ShowOldPosts