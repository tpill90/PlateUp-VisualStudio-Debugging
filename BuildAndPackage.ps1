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

Pop-Location