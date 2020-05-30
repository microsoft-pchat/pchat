$rootPath = $global:PSScriptRoot
if ($psISE) {
  $rootPath = Split-Path $psISE.CurrentFile.FullPath
} 
if ($rootPath -eq $null -or $rootPath -eq "")
{
  $rootPath = (Split-Path -Parent -Path $MyInvocation.MyCommand.Definition)
}

$serverIP = Get-Content -Path $rootPath\serverip -TotalCount 1

function showCaptureForm {
  param([string]$rootPath, [string]$hostname, [string]$port, [Guid]$id)

  $captureFormCsharpCode = [System.IO.File]::ReadAllText([System.IO.Path]::Combine($rootPath, "CaptureForm.cs"))
  Add-Type -Language CSharpVersion3 -TypeDefinition $captureFormCsharpCode -ReferencedAssemblies System.Drawing, System.Windows.Forms

  #Add-Type -AssemblyName System.Windows.Forms
  #Add-Type -AssemblyName System.Drawing

  [System.Windows.Forms.Application]::SetCompatibleTextRenderingDefault($false);
  [System.Windows.Forms.Application]::EnableVisualStyles();
  $form = New-Object WindowsFormsApp1.CaptureForm
  $form.Host = $hostname
  $form.Port = $port
  $form.Id = $id
  $form.TopLevel = $true
  $form.TopMost = $true
  $form.ShowDialog()

  Write-Host $form
}

# You will have to add a file called serverip in the same directory with the ipaddress of the server
$captureJob = Start-Job $function:showCaptureForm -ArgumentList $rootPath, $serverIP, "8443", ([System.Guid]::NewGuid())

while ($true) {
  if ($captureJob.Finished.WaitOne(0)) {
    break
  }
}


#really helpful to debug
#$captureJob | Receive-Job
#Write-Host "done"
