#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
	[switch]
	$IsOfficialBuild
)

Add-Type -assembly "system.io.compression.filesystem"

#
# Main
#
if (!$Configuration) {
    $Configuration = if ($env:CI -or $IsOfficialBuild) { 'Release' } else { 'Debug' }
}

$rids = @(
    "win-x64",
    "linux-x64",
    "osx-x64"
)

foreach($id in $rids)
{
    $zipFile = "artifacts/nyancat-$id-$APPVEYOR_BUILD_NUMBER.zip" 

    if (Test-Path -Path $zipFile)
    {
        Write-Host "Artifact '$zipfile' already exists, removing ..." -ForegroundColor Cyan
        Remove-Item -Path "artifacts/nyancat-$id-$APPVEYOR_BUILD_NUMBER.zip"
    }

    Write-Host "Creating $zipFile" -ForegroundColor Cyan

    [System.IO.Compression.ZipFile]::CreateFromDirectory(
        ".build/bin/Nyancat/$Configuration/netcoreapp2.1/$id/publish",
        "artifacts/nyancat-$id-$APPVEYOR_BUILD_NUMBER.zip" 
    )
}

Write-Host 'Done' -ForegroundColor Magenta
