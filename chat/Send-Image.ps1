function Send-Image {
    Param (
        [string]$ChatDirectory
    )

    $ImageDirectory = $ChatDirectory + "\Images\"

    if (-not (Test-Path -LiteralPath $ImageDirectory)) {
        New-Item -Path $ImageDirectory -ItemType Directory
    }

    $ImageName = $ImageDirectory + [guid]::NewGuid().Guid

    $image = Get-Clipboard -format image
    $image.Save($ImageName)
}