#Gets the installation folder
$installDir = $OctopusParameters["OctopusOriginalPackageDirectoryPath"]
$pscriptDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition

Write-Host "Install Directory $installDir"
Write-Host "PS Script Directory $pscriptDir"

#Gets CSI PS Library
$csiOctoLib = [System.IO.Path]::Combine($pscriptDir, 'CsiOctoPackLib.ps1')
$csiPsLib = [System.IO.Path]::Combine($pscriptDir, 'CsiPsLib.ps1')

#Load CSI PS Library
. $csiOctoLib
. $csiPsLib

Kill-Process -ProcessName "WebApiTester"

