$rootPath = $global:PSScriptRoot

# Path to network shared drive where users have read and write permissions
$ServerShare = "$rootPath\chatRooms"

function Enter-Chat 
{
  param
  (
    [Parameter(Mandatory)]
    [string]
    $ChatRoomName,
    
    [string]
    $UserName = $env:USERNAME,
    
    [Switch]
    $ShowOldPosts,
    
    [string]
    $HomeShare = $ServerShare
    
  )

  if ($ShowOldPosts)
  {
    $Option = ''
  }
  else
  {
    $Option = '-Tail 0'
  }

  $Path = Join-Path -Path $HomeShare -ChildPath "$ChatRoomName.txt"
  $exists = Test-Path -Path $Path
  if ($exists -eq $false)
  {
    $null = New-Item -Path $Path -Force -ItemType File
  }

  $process = Start-Process -FilePath powershell -ArgumentList "-noprofile -windowstyle hidden -command Get-Content -Path '$Path' $Option -Wait | Out-GridView -Title 'Chat: [$ChatRoomName]'" -PassThru

  Write-Host "For help, enter: /help"
  "[$UserName entered the chat]" | Add-Content -Path $Path
  do
  {
    Write-Host "[$ChatRoomName]: " -ForegroundColor Green -NoNewline
    $inputText = Read-Host 
    
    $isHelpCommand = '/help' -contains $inputText
    $isStopCommand = '/quit','/exit','/stop','/leave' -contains $inputText
    if ($isHelpCommand -eq $true)
    {
        Write-Host "To quit, enter: /quit.
To start screenshare, enter: /share"
	}
    elseif ($isStopCommand -eq $false)
    {
      "[$UserName] $inputText" | Add-Content -Path $Path
    }
    $isShareCommand = '/share','/screen','/screenshare' -contains $inputText
    if ($isShareCommand -eq $true)
    {
      "[$UserName] (Starting Screenshare)" | Add-Content -Path $Path
      invoke-expression 'cmd /c start powershell -File "$rootPath\screenshare\Friday.ps1"'
    }
  } until ($isStopCommand -eq $true)
  "[$UserName left the chat]" | Add-Content -Path $Path
  
  $process | Stop-Process
}

function Get-ChatRoom
{
  param
  (
    $HomeShare = $ServerShare
  )

  Get-ChildItem -Path $HomeShare -Filter *.txt -File |
    ForEach-Object {
      [PSCustomObject]@{
        RoomName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)
        LastActive = $_.LastWriteTime
        Started = $_.CreationTime
      }
    }
}