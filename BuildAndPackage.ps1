# TODO clean repo first
Push-Location $PSScriptRoot

mkdir ./dist

# Building BepinEx
dotnet run --project BepinEx/build/Build.csproj -- --target MakeDist
Copy-Item -Recurse BepinEx/bin/dist/Unity.Mono-win-x64/. ./dist/Bepinex

Pop-Location