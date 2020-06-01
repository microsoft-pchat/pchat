$rootPath = $global:PSScriptRoot

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

#Write The Chat Rooms
Write-Host ""
Write-Host "Existing Chat Rooms"
Write-Host "--------------------"

Try {
    (Split-Path -Path "$rootPath\chatRooms\*.txt" -Leaf -Resolve).Replace(".txt","")
}

Catch {

 Write-Host "NONE"

}
Write-Host ""

#Get 
$ChatRoom = Read-Host -Prompt 'Chat Room to join/create (leave blank for auto-generated room name)'

If ($ChatRoom -eq "") {
    $ChatRoom = (Get-RandomAlphanumericString | Tee-Object -variable teeTime).ToLower()
    Write-Host "Room Name: $ChatRoom"
}

Import-Module $rootPath\chat.ps1
Enter-Chat -ChatRoomName $ChatRoom -UserName ($env:USERNAME).ToUpper() -ShowOldPosts