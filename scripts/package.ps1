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

[xml]$manifest = Get-Content "$PSScriptRoot/../Version.props"
$version = "$($manifest.Project.PropertyGroup.VersionPrefix)"

foreach($id in $rids)
{
    $path = (Get-Location).Path
    $zipFile = "$path/artifacts/nyancat.$id.$version.zip" 

    try
    {

        if (Test-Path -Path $zipFile)
        {
            Write-Host "Artifact '$zipfile' already exists, removing ..." -ForegroundColor Cyan
            Remove-Item -Path $zipFile
        }
    
        Write-Host "Creating $zipFile" -ForegroundColor Cyan
    
        [System.IO.Compression.ZipFile]::CreateFromDirectory(
            ".build/bin/Nyancat/$Configuration/netcoreapp2.1/$id/publish",
            $zipFile
        )
    }
    catch
    {
        Write-Error "Something went wrong!" -ForegroundColor Red
        exit 1
    }
}

Write-Host 'Done' -ForegroundColor Magenta
