# TODO put this into a github action job at some point
Push-Location $PSScriptRoot

# git clean -fxd
mkdir ./DebuggingTools -ErrorAction SilentlyContinue

# Building BepinEx
dotnet run --project BepinEx/build/Build.csproj -- --target MakeDist
Copy-Item -Recurse BepinEx/bin/dist/Unity.Mono-win-x64/. ./DebuggingTools/Bepinex

# Copying libraries and scripts
Copy-Item -Recurse libraries ./DebuggingTools/Libraries
Copy-Item -Recurse Scripts/* ./DebuggingTools

# Building the plugin and copying it to the right output dir
dotnet build DebuggerShimPlugin/DebuggerShimPlugin.csproj --output ./DebuggerShimPlugin/output
Copy-Item ./DebuggerShimPlugin/output/DebuggerShimPlugin-Workshop.dll ./DebuggingTools/BepinEx/BepInEx/plugins

# Creating the final zip
Compress-Archive ./DebuggingTools -DestinationPath ReleasePackage.zip -Force

Pop-Location