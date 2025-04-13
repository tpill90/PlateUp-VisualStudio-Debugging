# TODO clean repo first
Push-Location $PSScriptRoot

Remove-Item ./dist -Force -Recurse
mkdir ./dist -ErrorAction SilentlyContinue

# Building BepinEx
dotnet run --project BepinEx/build/Build.csproj -- --target MakeDist
Copy-Item -Recurse BepinEx/bin/dist/Unity.Mono-win-x64/. ./dist/Bepinex

# Copying libraries and scripts
Copy-Item -Recurse libraries ./dist/Libraries
Copy-Item -Recurse Scripts/* ./dist

# Building the plugin and copying it to the right output dir
dotnet build DebuggerShimPlugin/DebuggerShimPlugin.csproj --output ./DebuggerShimPlugin/output
Copy-Item ./DebuggerShimPlugin/output/DebuggerShimPlugin-Workshop.dll ./dist/BepinEx/BepInEx/plugins

# Creating the final zip
Compress-Archive ./dist/* -DestinationPath ReleasePackage.zip -Force

Pop-Location