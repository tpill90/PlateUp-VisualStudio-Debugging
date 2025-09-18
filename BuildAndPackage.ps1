# TODO put this into a github action job at some point
$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot

git clean -fxd -e .vs
mkdir ./DebuggingTools -ErrorAction SilentlyContinue | Out-Null

# Building BepInEx
dotnet run --project BepInEx/build/Build.csproj -- --target MakeDist
Copy-Item -Recurse BepInEx/bin/dist/Unity.Mono-win-x64/. ./DebuggingTools/BepInEx

# Copying libraries and scripts
Copy-Item -Recurse libraries ./DebuggingTools/Libraries
Copy-Item -Recurse Scripts/* ./DebuggingTools

# Building the plugin and copying it to the right output dir
dotnet build DebuggerShimPlugin/DebuggerShimPlugin.csproj --output ./DebuggerShimPlugin/output
Copy-Item ./DebuggerShimPlugin/output/DebuggerShimPlugin-Workshop.dll ./DebuggingTools/BepInEx/BepInEx/plugins

# Creating the final zip
Compress-Archive ./DebuggingTools -DestinationPath ReleasePackage.zip -Force

Pop-Location