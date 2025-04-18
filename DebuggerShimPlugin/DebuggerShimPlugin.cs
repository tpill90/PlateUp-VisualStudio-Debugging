﻿using System;
using System.Diagnostics;

namespace DebuggerShimPlugin
{
    [UsedImplicitly]
    [BepInPlugin($"tpill90.{ModInfo.ModName}", ModInfo.ModName, ModInfo.ModVersion)]
    public class DebuggerShimPlugin : BaseUnityPlugin
    {
        public static ManualLogSource log;

        [UsedImplicitly]
        public void Awake()
        {            log = Logger;
            log.LogInfo("Loading BepinEx Debugger Shim");

            var plateUpAppDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\PlateUp\\PlateUp";
            var modsDir = Path.Combine(plateUpAppDir, "Mods");
            var pdb2mdbPath = Path.Combine(plateUpAppDir, "Libraries/pdb2mdb.exe");

            // Finding all .dlls
            var directoryInfo = new DirectoryInfo(modsDir);
            var filesInDir = directoryInfo.GetFiles("*.dll", SearchOption.AllDirectories)
                                          .ToList();

            // Dynamically loading each mod here prior to the game attempting to load the mod itself.
            // This allows us to debug mods because Bepinex will pull the mods into its AppDomain, and will correctly read the .mdb debug symbols
            log.LogInfo($"Found {filesInDir.Count} mod .dlls");
            foreach (var file in filesInDir)
            {
                var fileNameNoExtension = Path.GetFileNameWithoutExtension(file.FullName);
                var pdbPath = $"{file.Directory}\\{fileNameNoExtension}.pdb";

                // Can't generate an .mdb if there isn't a .pdb
                if (!File.Exists(pdbPath))
                {
                    log.LogError($"pdb not found for {file.Name}.  Will not be able to debug in Visual Studio.  " +
                                  $"The .pdb must be generated as a 'Full' .pdb and copied into the mod directory.");
                }

                if (File.Exists(pdbPath))
                {
                    GenerateMdb(file, pdbPath, pdb2mdbPath);
                }

                Assembly assembly = Assembly.LoadFrom(file.FullName);
                log.LogInfo($"Loaded {assembly.FullName}");
            }
        }

        //TODO refactor this signature + general refactor + comment
        private static void GenerateMdb(FileInfo file, string pdbPath, string pdbToMdbPath)
        {
            var fileNameNoExtension = Path.GetFileNameWithoutExtension(file.FullName);
            FileInfo pdbFileInfo = new FileInfo($"{file.Directory}\\{fileNameNoExtension}.pdb");
            FileInfo mdbFileInfo = new FileInfo($"{file.Directory}\\{fileNameNoExtension}.dll.mdb");

            if (mdbFileInfo.Exists && pdbFileInfo.LastWriteTime < mdbFileInfo.LastWriteTime)
            {
                return;
            }

            var process = new Process
            {
                StartInfo =
                {
                    FileName = pdbToMdbPath,
                    Arguments = $"\"{file.FullName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            process.WaitForExit();

            log.LogWarning($"Generated new .mdb for {file.Name}");
        }
    }
}