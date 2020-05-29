function Send-Image {
    Param (
        [string]$ChatDirectory
    )
    $Paragraph.Inlines.Add((New-ChatMessage -Message "Add-Content to file 349" -ForeGround Red))
    $ImageDirectory = $ChatDirectory + "\Images\"

    if (-not (Test-Path -LiteralPath $ImageDirectory)) {
        New-Item -Path $ImageDirectory -ItemType Directory
    }

    $ImageName = $ImageDirectory + [guid]::NewGuid().Guid

    $image = Get-Clipboard -format image
    $image.Save($ImageName)
}