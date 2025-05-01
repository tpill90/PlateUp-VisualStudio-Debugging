# Notes
# TODO document that this will show no output if things are already installed
# TODO write an uninstall script as well
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot

# TODO can I detect this in some way from Steam?  Can I detect Steam's install dir?
# Getting PlateUp install dir
# TODO change this to use the current dir where the debug tools are installed
$plateupInstallDir = "C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp"
if (-Not(Test-Path "$plateupInstallDir\PlateUp.exe" -ErrorAction SilentlyContinue))
{
    # TODO detect that the folder was copied into the wrong directory
    Write-Error "Cannot detect Plate Up install at $plateupInstallDir.  Stopping..."
    return
}

# Installs Bepinex.  This is a custom build that uses the exact same version of HarmonyX that is currently in the workshop.
# If this version of HarmonyX isn't used then there will be a conflict and the game won't load mods correctly.
if(-Not(Test-Path "$plateupInstallDir\BepInEx"))
{
    Copy-Item -LiteralPath "$plateupInstallDir\DebuggingTools\BepInEx" -Destination $plateupInstallDir -Force -Recurse
    Write-Host "Installed BepInEx" -ForegroundColor Yellow
}

# Copies over the debug versions of Unity which will turn the "Release" build back into a debuggable version
# Copy-Item .\libraries\UnityPlayer.exe -Destination "$plateupInstallDir\PlateUp\UnityPlayer.exe" -Force
# Copy-Item .\libraries\UnityPlayer.dll -Destination "$plateupInstallDir\PlateUp" -Force
# Copy-Item .\libraries\WinPixEventRuntime.dll -Destination "$plateupInstallDir\PlateUp" -Force

# # This configures Unity to allow debugger connections
# $containsText = (Get-Content "$plateupInstallDir\PlateUp\PlateUp_Data\boot.config" -Raw).Contains(("player-connection-debug=1"))
# if(-Not($containsText))
# {
#     Add-Content "$plateupInstallDir\PlateUp\PlateUp_Data\boot.config" "player-connection-debug=1"
# }

# Write-Host -ForegroundColor Yellow "Debugging environment setup!"

# Pop-Location

# TODO change WriteUnityLog = false to true