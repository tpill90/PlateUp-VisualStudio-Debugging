# Notes
# TODO document that this will show no output if things are already installed
# TODO write an uninstall script as well
# TODO should probably have this automatically download the latest version
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot

# TODO can I detect this in some way from Steam?  Can I detect Steam's install dir?
# Getting PlateUp install dir
# TODO change this to use the current dir where the debug tools are installed
# TODO have this possibly as a parameter that can be specified.
$plateupInstallDir = "C:\Program Files (x86)\Steam\steamapps\common\PlateUp\PlateUp"
if (-Not(Test-Path "$plateupInstallDir\PlateUp.exe" -ErrorAction SilentlyContinue))
{
    # TODO detect that the folder was copied into the wrong directory
    Write-Error "Cannot detect Plate Up install at $plateupInstallDir.  Stopping..."
    return
}

# Installs Bepinex.  This is a custom build that uses the exact same version of HarmonyX that is currently in the workshop.
# If this version of HarmonyX isn't used then there will be a conflict and the game won't load mods correctly.
if (-Not(Test-Path "$plateupInstallDir\BepInEx"))
{
    Copy-Item .\BepInEx\* -Destination $plateupInstallDir -Force -Recurse
    Write-Host "Installed BepInEx" -ForegroundColor Yellow
}

# Copies over the debug versions of Unity which will turn the "Release" build back into a debuggable version
#TODO doesn't appear that I need unityplayer.exe so remove that from the build package.
# UnityPlayer.dll
if (-Not(Test-Path "$plateupInstallDir\UnityPlayer.dll.original" -ErrorAction SilentlyContinue))
{
    Copy-Item "$plateupInstallDir\UnityPlayer.dll" -Destination "$plateupInstallDir\UnityPlayer.dll.original"
    Copy-Item .\Libraries\UnityPlayer.dll -Destination "$plateupInstallDir\UnityPlayer.dll" -Force

    Write-Host "Copied debug UnityPlayer.dll" -ForegroundColor Yellow
}
# WinPixEventRuntime.dll
if (-Not(Test-Path "$plateupInstallDir\WinPixEventRuntime.dll" -ErrorAction SilentlyContinue))
{
    Copy-Item .\Libraries\WinPixEventRuntime.dll -Destination "$plateupInstallDir\WinPixEventRuntime.dll" -Force

    Write-Host "Copied WinPixEventRuntime.dll" -ForegroundColor Yellow
}

# This configures Unity to allow debugger connections
$containsText = (Get-Content "$plateupInstallDir\PlateUp_Data\boot.config" -Raw).Contains(("player-connection-debug=1"))
if (-Not($containsText))
{
    Add-Content "$plateupInstallDir\PlateUp_Data\boot.config" "player-connection-debug=1"
    Write-Host "Configured Unity to accept debugger connections" -ForegroundColor Yellow
}


# Pop-Location

# TODO change WriteUnityLog = false to true