# Notes
# mono-2.0-bdwgc.dll does not seem to be needed.  But I'm not going to delete it just yet because it was difficult to compile.
# TODO write an uninstall script as well

Push-Location $PSScriptRoot

# Getting PlateUp install dir
[xml]$parsedProps = Get-Content ..\Environment.props
$plateupInstallDir = ([string]$parsedProps.Project.PropertyGroup.PlateUpInstallDir).Trim()
if (-Not(Test-Path $plateupInstallDir -ErrorAction SilentlyContinue))
{
    Write-Error "Cannot detect Plate Up install at $plateupInstallDir.  Stopping..."
    return
}

# Installs Bepinex.  This is a custom build that uses the exact same version of HarmonyX that is currently in the workshop.
# If this version of HarmonyX isn't used then there will be a conflict and the game won't load mods correctly.
if(-Not(Test-Path "$plateupInstallDir\PlateUp\BepInEx"))
{
    Expand-Archive -LiteralPath .\libraries\BepInEx.zip -DestinationPath "$plateupInstallDir\PlateUp" -Force
}

# Copies over the debug versions of Unity which will turn the "Release" build back into a debuggable version
Copy-Item .\libraries\UnityPlayer.exe -Destination "$plateupInstallDir\PlateUp\UnityPlayer.exe" -Force
Copy-Item .\libraries\UnityPlayer.dll -Destination "$plateupInstallDir\PlateUp" -Force
Copy-Item .\libraries\WinPixEventRuntime.dll -Destination "$plateupInstallDir\PlateUp" -Force

# This configures Unity to allow debugger connections
$containsText = (Get-Content "$plateupInstallDir\PlateUp\PlateUp_Data\boot.config" -Raw).Contains(("player-connection-debug=1"))
if(-Not($containsText))
{
    Add-Content "$plateupInstallDir\PlateUp\PlateUp_Data\boot.config" "player-connection-debug=1"
}

Write-Host -ForegroundColor Yellow "Debugging environment setup!"

Pop-Location